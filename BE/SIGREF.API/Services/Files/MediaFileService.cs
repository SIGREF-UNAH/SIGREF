using Microsoft.EntityFrameworkCore;
using SIGREF.API.Database;
using SIGREF.API.Database.Entity.common;
using SIGREF.API.Database.Entity.Files;
using SIGREF.API.Dtos.Common;
using SIGREF.API.Dtos.Files;
using SIGREF.API.Helpers;
using SIGREF.API.Services.Auth;


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


    // este enpoint seria solo para logo y logo de salud
    public async Task<ResponseDto<MediaFileDto>> UploadAsync(UploadMediaFileDto dto)
    {
        var response = new ResponseDto<MediaFileDto>();

        try
        {
            var file = dto.File;

            // ================= VALIDACIONES =================

            if (!Enum.IsDefined(typeof(MediaFileType), dto.Type))
            {
                response.Status = false;
                response.Message = "Tipo de media inválido.";
                response.StatusCode = 400;
                return response;
            }

            var systemFolder = MediaPathHelper.GetFolder(dto.Type);

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(ext) || !MediaHelper.AllowedImageExtensions.Contains(ext))
            {
                response.Status = false;
                response.Message = "Solo se aceptan imágenes JPG o PNG.";
                response.StatusCode = 400;
                return response;
            }

            var allowedContentTypes = new[] { "image/jpeg", "image/png" };
            if (!allowedContentTypes.Contains(file.ContentType))
            {
                response.Status = false;
                response.Message = "Content-Type inválido. Solo JPG o PNG.";
                response.StatusCode = 400;
                return response;
            }

            const long MAX_SIZE_BYTES = 25 * 1024 * 1024;
            if (file.Length > MAX_SIZE_BYTES)
            {
                response.Status = false;
                response.Message = "El archivo excede los 25 MB permitidos.";
                response.StatusCode = 400;
                return response;
            }

            // ================= TRANSACCIÓN CON EXECUTION STRATEGY =================

            var strategy = _context.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
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
                var finalFileName = $"{entity.Id}_{timestamp}_{dto.Type}{ext}";

                var basePath = Path.Combine(_env.ContentRootPath, "media", systemFolder);
                if (!Directory.Exists(basePath))
                    Directory.CreateDirectory(basePath);

                var finalPhysicalPath = Path.Combine(basePath, finalFileName);

                try
                {
                    using var stream = new FileStream(finalPhysicalPath, FileMode.Create);
                    await file.CopyToAsync(stream);
                }
                catch
                {
                    _context.MediaFiles.Remove(entity);
                    await _context.SaveChangesAsync();
                    await transaction.RollbackAsync();

                    response.Status = false;
                    response.Message = "No se pudo guardar la imagen en el servidor.";
                    response.StatusCode = 500;
                    return;
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

                // respuesta final
                response.Status = true;
                response.StatusCode = 200;
                response.Message = "Archivo subido correctamente.";
                response.Data = new MediaFileDto
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

            return response;
        }
        catch (Exception ex)
        {
            response.Status = false;
            response.Message = $"Error inesperado: {ex.Message} ";
            //| {ex.StackTrace} | {ex.InnerException?.Message}
            response.StatusCode = 500;
            return response;
        }
    }


    public async Task<ResponseDto<MediaFileDto>> GetByIdAsync(Guid id)
    {
        try
        {
            var entity = await _context.MediaFiles
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
            {
                return new ResponseDto<MediaFileDto>
                {
                    Status = false,
                    StatusCode = 404,
                    Message = "Archivo no encontrado.",
                    Data = null
                };
            }

            return new ResponseDto<MediaFileDto>
            {
                Status = true,
                StatusCode = 200,
                Message = "Archivo encontrado.",
                Data = new MediaFileDto
                {
                    Id = entity.Id,
                    FileName = entity.FileName,
                    ContentType = entity.ContentType,
                    RelativePath = entity.RelativePath,
                    SizeBytes = entity.SizeBytes,
                    Description = entity.Description,
                    Type = entity.Type,
                    SystemDescription = entity.SystemDescription
                }
            };
        }
        catch (Exception ex)
        {
            return new ResponseDto<MediaFileDto>
            {
                Status = false,
                StatusCode = 500,
                Message = $"Error al buscar archivo: {ex.Message}",
                Data = null
            };
        }
    }


    public async Task<bool> DeleteAsync(Guid id)
    {
        try
        {
            var entity = await _context.MediaFiles.FindAsync(id);
            if (entity == null)
                return false;

            // =============================
            // 1. ELIMINAR REFERENCIAS EN HOSPITAL PROPERTIES
            // =============================
            var hospital = await _context.HospitalProperties
                .FirstOrDefaultAsync(x => x.IsSingleton);

            if (hospital != null)
            {
                bool changed = false;

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
                    hospital.UpdatedDate = DateTime.Now;
                    await _context.SaveChangesAsync();
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

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error eliminando media: {ex.Message} | {ex.StackTrace}");

            return false;
        }
    }


    public async Task<ResponseDto<bool>> SetHospitalMediaAsync(Guid mediaId, MediaFileType type)
    {
        try
        {
            // Validar tipo permitido
            if (type != MediaFileType.AppHospital && type != MediaFileType.HealthGuilt)
            {
                return new ResponseDto<bool>
                {
                    Status = false,
                    StatusCode = 400,
                    Message = "Tipo de archivo inválido para el hospital.",
                    Data = false
                };
            }

            // Validar que el archivo exista
            var media = await _context.MediaFiles
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == mediaId);

            if (media == null)
            {
                return new ResponseDto<bool>
                {
                    Status = false,
                    StatusCode = 404,
                    Message = "El archivo no existe.",
                    Data = false
                };
            }

            // Obtener configuración del hospital (singleton)
            var hospital = await _context.HospitalProperties
                .FirstOrDefaultAsync(x => x.IsSingleton);

            if (hospital == null)
            {
                return new ResponseDto<bool>
                {
                    Status = false,
                    StatusCode = 404,
                    Message = "No existe una configuración del hospital.",
                    Data = false
                };
            }

            // Actualizar segun tipo
            string publicUrl = media.RelativePath; // aqui ya es /media/...

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

            // Guardar cambios
            await _context.SaveChangesAsync();

            return new ResponseDto<bool>
            {
                Status = true,
                StatusCode = 200,
                Message = "Archivo asignado correctamente al hospital.",
                Data = true
            };
        }
        catch (Exception ex)
        {
            return new ResponseDto<bool>
            {
                Status = false,
                StatusCode = 500,
                Message = $"Error inesperado: {ex.Message}",
                Data = false
            };
        }
    }


    public async Task<ResponseDto<PagedResultDto<MediaFileDto>>> GetPagedAsync(MediaFileFilterDto filter)
    {
        try
        {
            // Validaciones básicas
            if (filter.PageNumber <= 0) filter.PageNumber = 1;
            if (filter.PageSize <= 0) filter.PageSize = 20;

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
            var totalPages = (int)Math.Ceiling(totalItems / (double)filter.PageSize);
            if (totalPages == 0) totalPages = 1;
            if (filter.PageNumber > totalPages) filter.PageNumber = totalPages;

            // Aplicar paginación
            var items = await query
                .OrderByDescending(x => x.CreatedDate)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
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

            // respons
            return new ResponseDto<PagedResultDto<MediaFileDto>>
            {
                Status = true,
                StatusCode = 200,
                Message = "Imágenes obtenidas correctamente.",
                Data = new PagedResultDto<MediaFileDto>
                {
                    Items = items,
                    Pagination = new PaginationDto
                    {
                        CurrentPage = filter.PageNumber,
                        PageSize = filter.PageSize,
                        TotalItems = totalItems,
                        TotalPages = totalPages,
                        HasPrevious = filter.PageNumber > 1,
                        HasNext = filter.PageNumber < totalPages
                    }
                }
            };
        }
        catch (Exception ex)
        {
            return new ResponseDto<PagedResultDto<MediaFileDto>>
            {
                Status = false,
                StatusCode = 500,
                Message = $"Error inesperado: {ex.Message}",
                Data = null
            };
        }
    }
}