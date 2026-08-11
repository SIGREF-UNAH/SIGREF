using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Microsoft.EntityFrameworkCore;
using SIGREF.API.Dtos.Cashier;
using SIGREF.API.Dtos.Common;
using SIGREF.API.Extensions;
using SIGREF.API.Helpers;
using SIGREF.API.Middleware;
using SIGREF.Common.Dtos;
using SIGREF.Common.Exceptions;
using SIGREF.Core.Entity.Cashier;
using SIGREF.Infrastructure.Keycloak.Interfaces;
using SIGREF.Infrastructure.Persistence;
using FhirLocation = Hl7.Fhir.Model.Location;
using Task = System.Threading.Tasks.Task;

namespace SIGREF.API.Services.Cashier;

public class ShiftService : IShiftService
{
    private readonly SIGREFContext _db;
    private readonly FhirClient _fhirClient;
    private readonly IUserContextService _userContext;

    public ShiftService(SIGREFContext db, FhirClient fhirClient, IUserContextService userContext)
    {
        _db = db;
        _fhirClient = fhirClient;
        _userContext = userContext;
    }

    public async Task<ShiftDto> CreateShiftAsync(CreateShiftDto dto)
    {
        try
        {
            // =======================================================
            // VALIDAR QUE EL LOCATION EXISTA EN FHIR
            // =======================================================
            Bundle results;
            try
            {
                results = await _fhirClient.SearchByIdAsync<FhirLocation>(dto.LocationId);
            }
            catch (FhirOperationException fhirEx)
            {
                throw FhirExceptionMapper.Map(fhirEx, dto.LocationId, nameof(CreateShiftAsync));
            }

            if (results == null || !results.Entry.Any())
            {
                throw new NotFoundException("FHIR_LOCATION_NOT_FOUND", new Dictionary<string, object>
                {
                    { "LocationId", dto.LocationId }
                });
            }

            // =======================================================
            // VALIDAR EXISTENCIA DE NOMBRE DUPLICADO
            // =======================================================
            var normalizedName = dto.Name.Trim().ToUpper();

            var existsSameName = await _db.Shifts.AnyAsync(s =>
                s.IsActive &&
                s.LocationId == dto.LocationId &&
                s.Name.ToUpper() == normalizedName
            );

            if (existsSameName)
                throw new ConflictException("SHIFT_DUPLICATE_NAME", new Dictionary<string, object>
                {
                    { "Name", dto.Name },
                    { "LocationId", dto.LocationId }
                });

            // =======================================================
            // VALIDAR EXISTENCIA DE MISMA HORA DE INICIO
            // =======================================================
            var existsSameStartTime = await _db.Shifts.AnyAsync(s =>
                s.IsActive &&
                s.LocationId == dto.LocationId &&
                s.StartTime == dto.StartTime
            );

            if (existsSameStartTime)
                throw new ConflictException("SHIFT_DUPLICATE_START_TIME", new Dictionary<string, object>
                {
                    { "StartTime", dto.StartTime },
                    { "LocationId", dto.LocationId }
                });

            // =======================================================
            // CREAR ENTIDAD
            // =======================================================
            var newShift = new ShiftEntity
            {
                LocationId = dto.LocationId,
                Name = dto.Name,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                IsActive = true,
                CreatedById = _userContext.GetUserId(),
                CreatedDate = DateTime.UtcNow
            };

            _db.Shifts.Add(newShift);
            await _db.SaveChangesAsync();

            return newShift.ToDto();
        }
        catch (FhirOperationException fhirEx)
        {
            throw FhirExceptionMapper.Map(fhirEx, dto.LocationId, nameof(CreateShiftAsync));
        }
        catch (DbUpdateException dbEx)
        {
            throw new ExternalServiceException("DB_CREATE_SHIFT_ERROR", 502, new Dictionary<string, object>
            {
                { "OriginalException", dbEx.Message },
                { "Operation", nameof(CreateShiftAsync) }
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
            throw new ExternalServiceException("INTERNAL_SHIFT_ERROR", 500, new Dictionary<string, object>
            {
                { "OriginalException", ex.Message },
                { "Operation", nameof(CreateShiftAsync) }
            });
        }
    }

    public async Task<ShiftDto> UpdateShiftAsync(Guid id, UpdateShiftDto dto)
    {
        try
        {
            // 1. BUSCAR EL TURNO
            var shift = await _db.Shifts.FirstOrDefaultAsync(s => s.Id == id);

            if (shift == null)
                throw new NotFoundException("SHIFT_NOT_FOUND", new Dictionary<string, object>
                {
                    { "ShiftId", id }
                });

            // 2. VALIDAR LOCATION SI SE ESTÁ CAMBIANDO
            if (!string.IsNullOrWhiteSpace(dto.LocationId) && dto.LocationId != shift.LocationId)
            {
                Bundle results;
                try
                {
                    results = await _fhirClient.SearchByIdAsync<FhirLocation>(dto.LocationId);
                }
                catch (FhirOperationException fhirEx)
                {
                    throw FhirExceptionMapper.Map(fhirEx, dto.LocationId, nameof(UpdateShiftAsync));
                }

                if (results == null || !results.Entry.Any())
                    throw new NotFoundException("FHIR_LOCATION_NOT_FOUND", new Dictionary<string, object>
                    {
                        { "LocationId", dto.LocationId }
                    });
            }

            var targetLocationId = dto.LocationId ?? shift.LocationId;

            // 3. VALIDAR NOMBRE DUPLICADO (solo si cambia y solo entre turnos ACTIVOS)
            if (!string.IsNullOrWhiteSpace(dto.Name) &&
                dto.Name.Trim().ToUpper() != shift.Name.Trim().ToUpper())
            {
                bool nameExists = await _db.Shifts.AnyAsync(s =>
                    s.Id != shift.Id &&
                    s.IsActive &&
                    s.LocationId == targetLocationId &&
                    s.Name.Trim().ToUpper() == dto.Name.Trim().ToUpper());

                if (nameExists)
                    throw new ConflictException("SHIFT_DUPLICATE_NAME", new Dictionary<string, object>
                    {
                        { "Name", dto.Name },
                        { "LocationId", targetLocationId }
                    });
            }

            // 4. VALIDAR HORA DE INICIO DUPLICADA (solo entre turnos ACTIVOS)
            if (dto.StartTime.HasValue && dto.StartTime.Value != shift.StartTime)
            {
                bool timeExists = await _db.Shifts.AnyAsync(s =>
                    s.Id != shift.Id &&
                    s.IsActive &&
                    s.LocationId == targetLocationId &&
                    s.StartTime == dto.StartTime.Value);

                if (timeExists)
                    throw new ConflictException("SHIFT_DUPLICATE_START_TIME", new Dictionary<string, object>
                    {
                        { "StartTime", dto.StartTime.Value },
                        { "LocationId", targetLocationId }
                    });
            }

            // 5. APLICAR LOS CAMBIOS
            Guid userId = _userContext.GetUserId();

            shift.ApplyUpdate(dto, userId);

            if (dto.IsActive.HasValue)
                shift.IsActive = dto.IsActive.Value;

            shift.UpdatedById = userId;
            shift.UpdatedDate = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return shift.ToDto();
        }
        catch (FhirOperationException fhirEx)
        {
            throw FhirExceptionMapper.Map(fhirEx, dto.LocationId ?? id.ToString(), nameof(UpdateShiftAsync));
        }
        catch (DbUpdateException dbEx)
        {
            throw new ExternalServiceException("DB_UPDATE_SHIFT_ERROR", 502, new Dictionary<string, object>
            {
                { "OriginalException", dbEx.Message },
                { "Operation", nameof(UpdateShiftAsync) },
                { "ShiftId", id }
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
            throw new ExternalServiceException("INTERNAL_SHIFT_ERROR", 500, new Dictionary<string, object>
            {
                { "OriginalException", ex.Message },
                { "Operation", nameof(UpdateShiftAsync) },
                { "ShiftId", id }
            });
        }
    }

    public async Task DeleteShiftAsync(Guid id)
    {
        try
        {
            // =======================================================
            // BUSCAR TURNO
            // =======================================================
            var shift = await _db.Shifts
                .FirstOrDefaultAsync(s => s.Id == id && s.IsActive);

            if (shift == null)
                throw new NotFoundException("SHIFT_NOT_FOUND", new Dictionary<string, object>
                {
                    { "ShiftId", id }
                });

            // =======================================================
            // SOFT DELETE
            // =======================================================
            Guid userId = _userContext.GetUserId();

            shift.IsActive = false;
            shift.UpdatedById = userId;
            shift.UpdatedDate = DateTime.UtcNow;

            await _db.SaveChangesAsync();
        }
        catch (FhirOperationException fhirEx)
        {
            throw FhirExceptionMapper.Map(fhirEx, id.ToString(), nameof(DeleteShiftAsync));
        }
        catch (DbUpdateException dbEx)
        {
            throw new ExternalServiceException("DB_DELETE_SHIFT_ERROR", 502, new Dictionary<string, object>
            {
                { "OriginalException", dbEx.Message },
                { "Operation", nameof(DeleteShiftAsync) },
                { "ShiftId", id }
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
            throw new ExternalServiceException("INTERNAL_SHIFT_ERROR", 500, new Dictionary<string, object>
            {
                { "OriginalException", ex.Message },
                { "Operation", nameof(DeleteShiftAsync) },
                { "ShiftId", id }
            });
        }
    }

    public async Task<ShiftDto> GetShiftByIdAsync(Guid id)
    {
        try
        {
            // =======================================================
            // 1. BUSCAR EL TURNO EN SIGREF
            // =======================================================
            var shift = await _db.Shifts
                .FirstOrDefaultAsync(s => s.Id == id && s.IsActive);

            if (shift == null)
                throw new NotFoundException("SHIFT_NOT_FOUND", new Dictionary<string, object>
                {
                    { "ShiftId", id }
                });

            var dto = shift.ToDto();

            // =======================================================
            // OBTENER EL NOMBRE DEL LOCATION DESDE FHIR
            // =======================================================
            try
            {
                var location = await _fhirClient.ReadAsync<FhirLocation>(
                    $"Location/{shift.LocationId}?_elements=id,name"
                );
                dto.NameLocation = location.Name;
            }
            catch (FhirOperationException fhirEx)
                when (fhirEx.Status == System.Net.HttpStatusCode.NotFound)
            {
                dto.NameLocation = null;
            }
            catch (FhirOperationException fhirEx)
            {
                throw FhirExceptionMapper.Map(fhirEx, shift.LocationId, nameof(GetShiftByIdAsync));
            }

            return dto;
        }
        catch (FhirOperationException fhirEx)
        {
            throw FhirExceptionMapper.Map(fhirEx, id.ToString(), nameof(GetShiftByIdAsync));
        }
        catch (DbUpdateException dbEx)
        {
            throw new ExternalServiceException("DB_GET_SHIFT_ERROR", 502, new Dictionary<string, object>
            {
                { "OriginalException", dbEx.Message },
                { "Operation", nameof(GetShiftByIdAsync) },
                { "ShiftId", id }
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
            throw new ExternalServiceException("INTERNAL_SHIFT_ERROR", 500, new Dictionary<string, object>
            {
                { "OriginalException", ex.Message },
                { "Operation", nameof(GetShiftByIdAsync) },
                { "ShiftId", id }
            });
        }
    }

    public async Task<PagedResultDto<ShiftDto>> GetFilteredShiftsAsync(ShiftFilterDto filter)
    {
        try
        {
            // =======================================================
            // 1. NORMALIZAR PAGINACIÓN
            // =======================================================
            int pageNumber = Math.Clamp(filter.PageNumber <= 0 ? 1 : filter.PageNumber, 1, int.MaxValue);
            int pageSize = Math.Clamp(filter.PageSize <= 0 ? 10 : filter.PageSize, 1, 50);

            IQueryable<ShiftEntity> query = _db.Shifts.AsNoTracking();

            // =======================================================
            // 2. FILTROS BÁSICOS (SIGREF)
            // =======================================================
            if (filter.IsActive.HasValue)
                query = query.Where(s => s.IsActive == filter.IsActive.Value);

            if (!string.IsNullOrWhiteSpace(filter.Name))
            {
                var name = filter.Name.Trim().ToUpper();
                query = query.Where(s => s.Name.ToUpper().Contains(name));
            }

            if (!string.IsNullOrWhiteSpace(filter.LocationId))
                query = query.Where(s => s.LocationId == filter.LocationId);

            // =======================================================
            // FILTRO POR LOCATION NAME (FHIR LOOKUP)
            // =======================================================
            if (!string.IsNullOrWhiteSpace(filter.LocationName))
            {
                var searchParams = new SearchParams()
                    .Where($"name:contains={filter.LocationName}")
                    .Select("_elements=id,name");

                Bundle fhirResults;
                try
                {
                    fhirResults = await _fhirClient.SearchAsync<FhirLocation>(searchParams);
                }
                catch (FhirOperationException fhirEx)
                {
                    throw FhirExceptionMapper.Map(fhirEx, filter.LocationName, nameof(GetFilteredShiftsAsync));
                }

                var fhirLocationIds = fhirResults.Entry
                    .Where(e => e.Resource is FhirLocation)
                    .Select(e => ((FhirLocation)e.Resource).Id)
                    .ToList();

                if (fhirLocationIds.Count == 0)
                {
                    return new PagedResultDto<ShiftDto>
                    {
                        Items = new List<ShiftDto>(),
                        Pagination = new PaginationDto
                        {
                            CurrentPage = pageNumber,
                            PageSize = pageSize,
                            TotalItems = 0,
                            TotalPages = 0
                        }
                    };
                }

                query = query.Where(s => fhirLocationIds.Contains(s.LocationId));
            }

            // =======================================================
            // 4. TOTAL DE REGISTROS
            // =======================================================
            int totalItems = await query.CountAsync();
            int totalPages = totalItems > 0
                ? (int)Math.Ceiling(totalItems / (double)pageSize)
                : 0;

            // =======================================================
            // 5. OBTENER SHIFTS PAGINADOS
            // =======================================================
            var shifts = await query
                .OrderBy(s => s.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // =======================================================
            // FHIR LOOKUP PARA NAMES
            // =======================================================
            var shiftLocationIds = shifts
                .Select(s => s.LocationId.ToString())
                .Distinct()
                .ToList();

            Dictionary<string, string?> locationNames = new();

            if (shiftLocationIds.Any())
            {
                try
                {
                    var searchParamsLocations = new SearchParams()
                        .Where($"_id={string.Join(",", shiftLocationIds)}")
                        .Select("_elements=id,name");

                    var bundle = await _fhirClient.SearchAsync<FhirLocation>(searchParamsLocations);

                    locationNames = bundle.Entry
                        .Select(e => e.Resource as FhirLocation)
                        .Where(loc => loc != null)
                        .ToDictionary(
                            loc => loc!.Id,
                            loc => loc!.Name
                        );
                }
                catch (FhirOperationException fhirEx)
                {
                    throw FhirExceptionMapper.Map(fhirEx, string.Join(",", shiftLocationIds), nameof(GetFilteredShiftsAsync));
                }
            }

            // =======================================================
            // MAPEAR SHIFTS A DTO
            // =======================================================
            var dtoList = shifts.Select(shift =>
            {
                var dto = shift.ToDto();
                dto.NameLocation = locationNames.TryGetValue(shift.LocationId, out var locName)
                    ? locName
                    : null;
                return dto;
            }).ToList();

            return new PagedResultDto<ShiftDto>
            {
                Items = dtoList,
                Pagination = new PaginationDto
                {
                    CurrentPage = pageNumber,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    TotalPages = totalPages,
                    HasPrevious = pageNumber > 1,
                    HasNext = pageNumber < totalPages
                }
            };
        }
        catch (FhirOperationException fhirEx)
        {
            throw FhirExceptionMapper.Map(fhirEx, "ShiftFilter", nameof(GetFilteredShiftsAsync));
        }
        catch (DbUpdateException dbEx)
        {
            throw new ExternalServiceException("DB_FILTER_SHIFTS_ERROR", 502, new Dictionary<string, object>
            {
                { "OriginalException", dbEx.Message },
                { "Operation", nameof(GetFilteredShiftsAsync) }
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
            throw new ExternalServiceException("INTERNAL_SHIFT_ERROR", 500, new Dictionary<string, object>
            {
                { "OriginalException", ex.Message },
                { "Operation", nameof(GetFilteredShiftsAsync) }
            });
        }
    }
}
