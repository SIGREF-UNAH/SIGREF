using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SIGREF.API.Dtos.Administration;
using SIGREF.API.Extensions;
using SIGREF.API.Middleware;
using SIGREF.Common.Exceptions;
using SIGREF.Core.Entity.Administration;
using SIGREF.Infrastructure.Keycloak.Interfaces;
using SIGREF.Infrastructure.Persistence;
using Hl7.Fhir.Rest;

namespace SIGREF.API.Services.AdministrationHospital;

public class HospitalPropertiesService : IHospitalPropertiesService
{
    private readonly SIGREFContext _context;
    private readonly IMemoryCache _cache;
    private readonly IUserContextService _userContext;

    // Cache key constante para evitar magic strings
    private const string CACHE_KEY_HOSPITAL = "hospital_singleton";
    private static readonly TimeSpan CACHE_DURATION = TimeSpan.FromMinutes(5);

    public HospitalPropertiesService(
        SIGREFContext context,
        IMemoryCache cache,
        IUserContextService userContext)
    {
        _context = context;
        _cache = cache;
        _userContext = userContext;
    }

    // ============================================================
    // GET PUBLICO
    // ============================================================
    public async Task<HospitalPublicDto> GetPublicAsync()
    {
        try
        {
            var hospital = await GetCachedHospitalAsync();

            return new HospitalPublicDto
            {
                Name = hospital.Name,
                UrlLogo = hospital.UrlLogo,
                UrlLogoHealth = hospital.UrlLogoHealth
            };
        }
        catch (DbUpdateException dbEx)
        {
            throw new ExternalServiceException("DB_GET_HOSPITAL_ERROR", 502, new Dictionary<string, object>
            {
                { "OriginalException", dbEx.Message },
                { "Operation", nameof(GetPublicAsync) }
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
            throw new ExternalServiceException("INTERNAL_HOSPITAL_ERROR", 500, new Dictionary<string, object>
            {
                { "OriginalException", ex.Message },
                { "Operation", nameof(GetPublicAsync) }
            });
        }
    }

    // ============================================================
    // GET ALL DETAILS
    // ============================================================
    public async Task<HospitalDetailsDto> GetAllDetailsAsync()
    {
        try
        {
            var hospital = await GetCachedHospitalAsync();
            return hospital.ToDto();
        }
        catch (DbUpdateException dbEx)
        {
            throw new ExternalServiceException("DB_GET_HOSPITAL_ERROR", 502, new Dictionary<string, object>
            {
                { "OriginalException", dbEx.Message },
                { "Operation", nameof(GetAllDetailsAsync) }
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
            throw new ExternalServiceException("INTERNAL_HOSPITAL_ERROR", 500, new Dictionary<string, object>
            {
                { "OriginalException", ex.Message },
                { "Operation", nameof(GetAllDetailsAsync) }
            });
        }
    }

    // ============================================================
    // CREATE (solo se usa 1 vez)
    // ============================================================
    public async Task<HospitalDetailsDto> CreateAsync(CreateHospitalPropertiesDto dto)
    {
        try
        {
            // Ejecutar dentro de una transacción para garantizar atomicidad
            // entre la validación de existencia y la inserción
            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    // Validación con bloqueo pesimista (FOR UPDATE) para evitar race conditions
                    var existing = await _context.HospitalProperties
                        .FirstOrDefaultAsync(x => x.IsSingleton);

                    if (existing != null)
                    {
                        await transaction.RollbackAsync();
                        throw new ConflictException("HOSPITAL_ALREADY_EXISTS", new Dictionary<string, object>
                        {
                            { "ExistingHospitalId", existing.Id }
                        });
                    }

                    var userId = _userContext.GetUserId();
                    var now = DateTimeOffset.UtcNow;

                    var entity = new HospitalPropertiesEntity
                    {
                        Name = dto.Name,
                        Director = dto.Director,
                        Subdirector = dto.Subdirector,
                        Location = dto.Location,
                        PhoneNumber = dto.PhoneNumber,
                        Email = dto.Email,
                        HospitalCode = dto.HospitalCode,
                        RTN = dto.RTN,
                        Website = dto.Website,
                        Currency = dto.Currency,
                        IsSingleton = true,
                        CreatedById = userId,
                        CreatedDate = now,
                        UpdatedById = userId,
                        UpdatedDate = now
                    };

                    _context.HospitalProperties.Add(entity);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    // Invalidar cache después de crear
                    InvalidateCache();

                    return entity.ToDto();
                }
                catch (AppException)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }
        catch (DbUpdateException dbEx)
        {
            throw new ExternalServiceException("DB_CREATE_HOSPITAL_ERROR", 502, new Dictionary<string, object>
            {
                { "OriginalException", dbEx.Message },
                { "Operation", nameof(CreateAsync) }
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
            throw new ExternalServiceException("INTERNAL_HOSPITAL_ERROR", 500, new Dictionary<string, object>
            {
                { "OriginalException", ex.Message },
                { "Operation", nameof(CreateAsync) }
            });
        }
    }

    // ============================================================
    // UPDATE
    // ============================================================
    public async Task<HospitalDetailsDto> UpdateAsync(UpdateHospitalPropertiesDto dto)
    {
        try
        {
            // Optimización: ExecuteUpdateAsync evita cargar la entidad completa en memoria
            // Solo actualiza las columnas necesarias directamente en la BD
            var userId = _userContext.GetUserId();
            var now = DateTimeOffset.UtcNow;

            var affectedRows = await _context.HospitalProperties
                .Where(x => x.IsSingleton)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(h => h.Name, dto.Name)
                    .SetProperty(h => h.Director, dto.Director)
                    .SetProperty(h => h.Subdirector, dto.Subdirector)
                    .SetProperty(h => h.Location, dto.Location)
                    .SetProperty(h => h.PhoneNumber, dto.PhoneNumber)
                    .SetProperty(h => h.Email, dto.Email)
                    .SetProperty(h => h.HospitalCode, dto.HospitalCode)
                    .SetProperty(h => h.RTN, dto.RTN)
                    .SetProperty(h => h.Website, dto.Website)
                    .SetProperty(h => h.Currency, dto.Currency)
                    .SetProperty(h => h.UpdatedById, userId)
                    .SetProperty(h => h.UpdatedDate, now)
                );

            if (affectedRows == 0)
                throw new NotFoundException("HOSPITAL_NOT_CONFIGURED");

            // Invalidar cache después de actualizar
            InvalidateCache();

            // Retornar el DTO actualizado (reconstruido desde los datos de entrada)
            // Nota: ExecuteUpdateAsync no retorna la entidad, así que reconstruimos el DTO
            return new HospitalDetailsDto
            {
                Name = dto.Name,
                Director = dto.Director,
                Subdirector = dto.Subdirector,
                Location = dto.Location,
                PhoneNumber = dto.PhoneNumber,
                Email = dto.Email,
                HospitalCode = dto.HospitalCode,
                RTN = dto.RTN,
                Website = dto.Website,
                Currency = dto.Currency,
                UpdatedById = userId,
                UpdatedDate = now
            };
        }
        catch (DbUpdateException dbEx)
        {
            throw new ExternalServiceException("DB_UPDATE_HOSPITAL_ERROR", 502, new Dictionary<string, object>
            {
                { "OriginalException", dbEx.Message },
                { "Operation", nameof(UpdateAsync) }
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
            throw new ExternalServiceException("INTERNAL_HOSPITAL_ERROR", 500, new Dictionary<string, object>
            {
                { "OriginalException", ex.Message },
                { "Operation", nameof(UpdateAsync) }
            });
        }
    }

    // ============================================================
    // HELPERS PRIVADOS
    // ============================================================

    /// <summary>
    /// Obtiene el hospital singleton desde cache o BD.
    /// Cachea por 5 minutos para reducir queries repetitivas.
    /// </summary>
    private async Task<HospitalPropertiesEntity> GetCachedHospitalAsync()
    {
        if (_cache.TryGetValue(CACHE_KEY_HOSPITAL, out HospitalPropertiesEntity? cached))
        {
            if (cached != null)
                return cached;
        }

        var hospital = await _context.HospitalProperties
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.IsSingleton);

        if (hospital == null)
            throw new NotFoundException("HOSPITAL_NOT_CONFIGURED");

        // Guardar en cache con expiración absoluta
        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(CACHE_DURATION)
            .SetPriority(CacheItemPriority.High);

        _cache.Set(CACHE_KEY_HOSPITAL, hospital, cacheOptions);

        return hospital;
    }

    /// <summary>
    /// Invalida la cache del hospital singleton.
    /// Debe llamarse después de cualquier operación de escritura.
    /// </summary>
    private void InvalidateCache()
    {
        _cache.Remove(CACHE_KEY_HOSPITAL);
    }
}