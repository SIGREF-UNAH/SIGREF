using Hl7.Fhir.Rest;
using Microsoft.EntityFrameworkCore;
using SIGREF.API.Dtos.Files;
using SIGREF.API.Middleware;
using SIGREF.Common.Dtos;
using SIGREF.Common.Exceptions;
using SIGREF.Common.Helpers;
using SIGREF.Common.Types;
using SIGREF.Core.Entity.Files;
using SIGREF.Infrastructure.Keycloak.Interfaces;
using SIGREF.Infrastructure.Persistence;

namespace SIGREF.API.Services.Files;

public class MediaFileService : IMediaFileService
{
    private readonly SIGREFContext _context;
    private readonly IWebHostEnvironment _env;
    private readonly IUserContextService _userContextService;

    public MediaFileService(SIGREFContext context, IWebHostEnvironment env, IUserContextService userContextService)
    {
        _context = context;
        _env = env;
        _userContextService = userContextService;
    }

    public async Task<MediaFileDto> UploadAsync(UploadMediaFileDto dto)
    {
        var file = dto.File;

        // ================= VALIDACIONES =================

        if (!Enum.IsDefined(typeof(MediaFileType), dto.Type))
            throw new ValidationException("MEDIA_INVALID_TYPE", new Dictionary<string, object>
            {
                { "ProvidedType", dto.Type },
                { "AllowedTypes", Enum.GetNames(typeof(MediaFileType)) }
            });

        var systemFolder = MediaPathHelper.GetFolder(dto.Type);

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(ext) || !MediaHelper.AllowedImageExtensions.Contains(ext))
            throw new ValidationException("MEDIA_INVALID_EXTENSION", new Dictionary<string, object>
            {
                { "ProvidedExtension", ext },
                { "AllowedExtensions", MediaHelper.AllowedImageExtensions }
            });

        var allowedContentTypes = new[] { "image/jpeg", "image/png" };
        if (!allowedContentTypes.Contains(file.ContentType))
            throw new ValidationException("MEDIA_INVALID_CONTENT_TYPE", new Dictionary<string, object>
            {
                { "ProvidedContentType", file.ContentType },
                { "AllowedContentTypes", allowedContentTypes }
            });

        const long MAX_SIZE_BYTES = 25 * 1024 * 1024;
        if (file.Length > MAX_SIZE_BYTES)
            throw new ValidationException("MEDIA_FILE_TOO_LARGE", new Dictionary<string, object>
            {
                { "MaxSizeBytes", MAX_SIZE_BYTES },
                { "ProvidedSizeBytes", file.Length },
                { "MaxSizeMB", 25 }
            });

        // ================= TRANSACCIÓN CON EXECUTION STRATEGY =================

        var strategy = _context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            var entity = new MediaFileEntity
            {
                FileName = file.FileName,
                ContentType = file.ContentType,
                Type = dto.Type,
                Description = dto.Description,
                SizeBytes = file.Length,
                SystemDescription = "PENDING",
                RelativePath = "Pending"
            };

            _context.MediaFiles.Add(entity);
            await _context.SaveChangesAsync(); // genera ID

            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var finalFileName = $"{entity.Id}{timestamp}{dto.Type}{ext}";

            var basePath = Path.Combine(_env.ContentRootPath, "media", systemFolder);
            if (!Directory.Exists(basePath))
                Directory.CreateDirectory(basePath);

            // Aseguramos que solo se tome el nombre del archivo, previniendo Path Traversal
            var safeFinalFileName = Path.GetFileName(finalFileName);
            var finalPhysicalPath = Path.Combine(basePath, safeFinalFileName);
            // ===============================================

            try
            {
                await using var stream = new FileStream(finalPhysicalPath, FileMode.Create);
                await file.CopyToAsync(stream);
            }
            catch (IOException ioEx)
            {
                _context.MediaFiles.Remove(entity);
                await _context.SaveChangesAsync();
                await transaction.RollbackAsync();

                throw new ExternalServiceException("MEDIA_FILE_SAVE_ERROR", 500, new Dictionary<string, object>
                {
                    { "FileName", file.FileName },
                    { "PhysicalPath", finalPhysicalPath },
                    { "OriginalException", ioEx.Message }
                });
            }

            entity.RelativePath = $"/media/{systemFolder}/{finalFileName}";
            entity.SystemDescription = $"{entity.Id}-{timestamp}-{file.FileName}";

            await _context.SaveChangesAsync();

            // ================= Actualizar hospital =================

            var hospital = await _context.HospitalProperties
                .FirstOrDefaultAsync(x => x.IsSingleton);

            if (hospital != null)
            {
                if (dto.Type == MediaFileType.AppHospital)
                {
                    hospital.LogoMediaId = entity.Id;
                    hospital.UrlLogo = entity.RelativePath;
                }
                else if (dto.Type == MediaFileType.HealthGuilt)
                {
                    hospital.HealthLogoMediaId = entity.Id;
                    hospital.UrlLogoHealth = entity.RelativePath;
                }

                await _context.SaveChangesAsync();
            }

            await transaction.CommitAsync();

            return new MediaFileDto
            {
                Id = entity.Id,
                FileName = entity.FileName,
                ContentType = entity.ContentType,
                Description = entity.Description,
                SystemDescription = entity.SystemDescription,
                Type = entity.Type,
                RelativePath = entity.RelativePath,
                SizeBytes = entity.SizeBytes
            };
        });
    }

    public async Task<MediaFileDto> GetByIdAsync(Guid id)
    {
        try
        {
            var entity = await _context.MediaFiles
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
                throw new NotFoundException("MEDIA_NOT_FOUND", new Dictionary<string, object>
                {
                    { "MediaId", id }
                });

            return new MediaFileDto
            {
                Id = entity.Id,
                FileName = entity.FileName,
                ContentType = entity.ContentType,
                RelativePath = entity.RelativePath,
                SizeBytes = entity.SizeBytes,
                Description = entity.Description,
                Type = entity.Type,
                SystemDescription = entity.SystemDescription
            };
        }
        catch (FhirOperationException fhirEx)
        {
            throw FhirExceptionMapper.Map(fhirEx, id.ToString(), nameof(GetByIdAsync));
        }
        catch (DbUpdateException dbEx)
        {
            throw new ExternalServiceException("DB_QUERY_ERROR", 502, new Dictionary<string, object>
            {
                { "OriginalException", dbEx.Message },
                { "Operation", nameof(GetByIdAsync) },
                { "MediaId", id }
            });
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (AppException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("INTERNAL_MEDIA_ERROR", 500, new Dictionary<string, object>
            {
                { "OriginalException", ex.Message },
                { "Operation", nameof(GetByIdAsync) },
                { "MediaId", id }
            });
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        try
        {
            var entity = await _context.MediaFiles.FindAsync(id);
            if (entity == null)
                throw new NotFoundException("MEDIA_NOT_FOUND", new Dictionary<string, object>
                {
                    { "MediaId", id }
                });

            // =============================
            // 1. ELIMINAR REFERENCIAS EN HOSPITAL PROPERTIES
            // =============================
            var hospital = await _context.HospitalProperties
                .FirstOrDefaultAsync(x => x.IsSingleton);

            if (hospital != null)
            {
                var changed = false;

                if (hospital.LogoMediaId == id)
                {
                    hospital.LogoMediaId = null;
                    hospital.UrlLogo = null;
                    changed = true;
                }

                if (hospital.HealthLogoMediaId == id)
                {
                    hospital.HealthLogoMediaId = null;
                    hospital.UrlLogoHealth = null;
                    changed = true;
                }

                if (changed)
                {
                    hospital.UpdatedById = _userContextService.GetUserId();
                    hospital.UpdatedDate = DateTime.UtcNow;
                }
            }

            // =============================
            // 2. BORRAR ARCHIVO FÍSICO
            // =============================
            var physicalPath = Path.Combine(_env.ContentRootPath, entity.RelativePath.TrimStart('/'));

            if (File.Exists(physicalPath))
                File.Delete(physicalPath);

            // =============================
            // 3. BORRAR DE LA BD
            // =============================
            _context.MediaFiles.Remove(entity);
            await _context.SaveChangesAsync();
        }
        catch (FhirOperationException fhirEx)
        {
            throw FhirExceptionMapper.Map(fhirEx, id.ToString(), nameof(DeleteAsync));
        }
        catch (DbUpdateException dbEx)
        {
            throw new ExternalServiceException("DB_DELETE_ERROR", 502, new Dictionary<string, object>
            {
                { "OriginalException", dbEx.Message },
                { "Operation", nameof(DeleteAsync) },
                { "MediaId", id }
            });
        }
        catch (IOException ioEx)
        {
            throw new ExternalServiceException("MEDIA_FILE_DELETE_ERROR", 500, new Dictionary<string, object>
            {
                { "OriginalException", ioEx.Message },
                { "Operation", nameof(DeleteAsync) },
                { "MediaId", id }
            });
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (AppException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("INTERNAL_MEDIA_ERROR", 500, new Dictionary<string, object>
            {
                { "OriginalException", ex.Message },
                { "Operation", nameof(DeleteAsync) },
                { "MediaId", id }
            });
        }
    }

    public async Task SetHospitalMediaAsync(Guid mediaId, MediaFileType type)
    {
        // Validar tipo permitido
        if (type != MediaFileType.AppHospital && type != MediaFileType.HealthGuilt)
            throw new ValidationException("MEDIA_INVALID_HOSPITAL_TYPE", new Dictionary<string, object>
            {
                { "ProvidedType", type },
                { "AllowedTypes", new[] { MediaFileType.AppHospital, MediaFileType.HealthGuilt } }
            });

        try
        {
            // Validar que el archivo exista
            var media = await _context.MediaFiles
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == mediaId);

            if (media == null)
                throw new NotFoundException("MEDIA_NOT_FOUND", new Dictionary<string, object>
                {
                    { "MediaId", mediaId }
                });

            // Obtener configuración del hospital (singleton)
            var hospital = await _context.HospitalProperties
                .FirstOrDefaultAsync(x => x.IsSingleton);

            if (hospital == null)
                throw new NotFoundException("HOSPITAL_CONFIG_NOT_FOUND");

            // Actualizar segun tipo
            var publicUrl = media.RelativePath;

            if (type == MediaFileType.AppHospital)
            {
                hospital.LogoMediaId = media.Id;
                hospital.UrlLogo = publicUrl;
            }
            else
            {
                hospital.HealthLogoMediaId = media.Id;
                hospital.UrlLogoHealth = publicUrl;
            }

            await _context.SaveChangesAsync();
        }
        catch (FhirOperationException fhirEx)
        {
            throw FhirExceptionMapper.Map(fhirEx, mediaId.ToString(), nameof(SetHospitalMediaAsync));
        }
        catch (DbUpdateException dbEx)
        {
            throw new ExternalServiceException("DB_UPDATE_ERROR", 502, new Dictionary<string, object>
            {
                { "OriginalException", dbEx.Message },
                { "Operation", nameof(SetHospitalMediaAsync) },
                { "MediaId", mediaId }
            });
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (AppException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("INTERNAL_MEDIA_ERROR", 500, new Dictionary<string, object>
            {
                { "OriginalException", ex.Message },
                { "Operation", nameof(SetHospitalMediaAsync) },
                { "MediaId", mediaId }
            });
        }
    }

    public async Task<PagedResultDto<MediaFileDto>> GetPagedAsync(MediaFileFilterDto filter)
    {
        // Validaciones básicas
        if (filter.PageNumber <= 0)
            throw new ValidationException("MEDIA_INVALID_PAGE_NUMBER", new Dictionary<string, object>
            {
                { "ProvidedPageNumber", filter.PageNumber },
                { "MinAllowed", 1 }
            });

        if (filter.PageSize <= 0)
            throw new ValidationException("MEDIA_INVALID_PAGE_SIZE", new Dictionary<string, object>
            {
                { "ProvidedPageSize", filter.PageSize },
                { "MinAllowed", 1 }
            });

        try
        {
            var query = _context.MediaFiles.AsNoTracking().AsQueryable();

            // Filtro por tipo
            if (filter.Type.HasValue)
                query = query.Where(x => x.Type == filter.Type.Value);

            // Filtro por búsqueda
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var search = filter.Search.Trim().ToLower();
                query = query.Where(x =>
                    x.FileName.ToLower().Contains(search) ||
                    (x.Description != null && x.Description.ToLower().Contains(search)) ||
                    (x.SystemDescription != null && x.SystemDescription.ToLower().Contains(search))
                );
            }

            // Total antes de paginar
            var totalItems = await query.CountAsync();

            // Calcular paginado
            var totalPages = totalItems > 0
                ? (int)Math.Ceiling(totalItems / (double)filter.PageSize)
                : 0;

            var currentPage = filter.PageNumber;
            if (totalPages > 0 && currentPage > totalPages)
                currentPage = totalPages;

            // Aplicar paginación
            var items = await query
                .OrderByDescending(x => x.CreatedDate)
                .Skip((currentPage - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(x => new MediaFileDto
                {
                    Id = x.Id,
                    FileName = x.FileName,
                    ContentType = x.ContentType,
                    RelativePath = x.RelativePath,
                    SizeBytes = x.SizeBytes,
                    Description = x.Description,
                    SystemDescription = x.SystemDescription,
                    Type = x.Type
                })
                .ToListAsync();

            return new PagedResultDto<MediaFileDto>
            {
                Items = items,
                Pagination = new PaginationDto
                {
                    CurrentPage = currentPage,
                    PageSize = filter.PageSize,
                    TotalItems = totalItems,
                    TotalPages = totalPages,
                    HasPrevious = currentPage > 1,
                    HasNext = currentPage < totalPages
                }
            };
        }
        catch (FhirOperationException fhirEx)
        {
            throw FhirExceptionMapper.Map(fhirEx, "MediaPaged", nameof(GetPagedAsync));
        }
        catch (DbUpdateException dbEx)
        {
            throw new ExternalServiceException("DB_QUERY_ERROR", 502, new Dictionary<string, object>
            {
                { "OriginalException", dbEx.Message },
                { "Operation", nameof(GetPagedAsync) }
            });
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (AppException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("INTERNAL_MEDIA_ERROR", 500, new Dictionary<string, object>
            {
                { "OriginalException", ex.Message },
                { "Operation", nameof(GetPagedAsync) }
            });
        }
    }
}