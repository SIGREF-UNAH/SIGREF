using Hl7.Fhir.Rest;
using FhirLocation = Hl7.Fhir.Model.Location;
using Microsoft.EntityFrameworkCore;
using SIGREF.API.Database;
using SIGREF.API.Database.Entity.Cashier;
using SIGREF.API.Dtos.Cashier;
using SIGREF.API.Dtos.Common;
using SIGREF.API.Extensions;
using SIGREF.API.Helpers;
using SIGREF.API.Services.Auth;

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

    public async Task<ResponseDto<ShiftDto>> CreateShiftAsync(CreateShiftDto dto)
    {
        try
        {
            // =======================================================
            // VALIDAR QUE EL LOCATION EXISTA EN FHIR 
            // =======================================================
            try
            {
                await _fhirClient.ReadAsync<Hl7.Fhir.Model.Location>(
                    $"Location/{dto.LocationId}?_elements=id"
                );
            }
            catch (FhirOperationException ex)
                when (ex.Status == System.Net.HttpStatusCode.NotFound)
            {
                return ResponseHelper.Fail<ShiftDto>(400, "La ubicación especificada no existe en el servidor FHIR.");
            }

            // =======================================================
            // VALIDAR EXISTENCIA DE NOMBRE DUPLICADO
            // =======================================================
            var existsSameName = await _db.Shifts.AnyAsync(s =>
                s.IsActive &&
                s.LocationId == dto.LocationId &&
                s.Name.ToUpper() == dto.Name.ToUpper()
            );

            if (existsSameName)
                return ResponseHelper.Fail<ShiftDto>(400,
                    "Ya existe un turno activo con este nombre en esta ubicación.");

            // =======================================================
            // VALIDAR EXISTENCIA DE MISMA HORA DE INICIO
            // =======================================================
            var existsSameStartTime = await _db.Shifts.AnyAsync(s =>
                s.IsActive &&
                s.LocationId == dto.LocationId &&
                s.StartTime == dto.StartTime
            );

            if (existsSameStartTime)
                return ResponseHelper.Fail<ShiftDto>(400,
                    "Ya existe un turno activo que inicia a la misma hora en esta ubicación.");

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

            return ResponseHelper.Success(201, "Turno creado exitosamente.", newShift.ToDto());
        }
        catch (Exception ex)
        {
            return ResponseHelper.Fail<ShiftDto>(500, $"Error interno al crear el turno: {ex.Message}");
        }
    }

    public async Task<ResponseDto<ShiftDto>> UpdateShiftAsync(Guid id, UpdateShiftDto dto)
    {
        try
        {
            // =======================================================
            // VALIDAR QUE EL TURNO EXISTA
            // =======================================================
            var shift = await _db.Shifts.FirstOrDefaultAsync(s => s.Id == id && s.IsActive);

            if (shift == null)
                return ResponseHelper.Fail<ShiftDto>(404, "El turno especificado no existe o está inactivo.");

            // =======================================================
            // VALIDAR LOCATION SOLO SI SE ACTUALIZA
            // =======================================================
            if (dto.LocationId.HasValue && dto.LocationId.Value != shift.LocationId)
            {
                try
                {
                    await _fhirClient.ReadAsync<Hl7.Fhir.Model.Location>(
                        $"Location/{dto.LocationId}?_elements=id"
                    );
                }
                catch (FhirOperationException ex)
                    when (ex.Status == System.Net.HttpStatusCode.NotFound)
                {
                    return ResponseHelper.Fail<ShiftDto>(400,
                        "La ubicación indicada no existe en el servidor FHIR.");
                }
            }

            // =======================================================
            // VALIDAR NOMBRE DUPLICADO (solo si cambia)
            // =======================================================
            if (!string.IsNullOrWhiteSpace(dto.Name) &&
                dto.Name.ToUpper() != shift.Name.ToUpper())
            {
                var existsSameName = await _db.Shifts.AnyAsync(s =>
                        s.IsActive &&
                        s.LocationId == (dto.LocationId ?? shift.LocationId) &&
                        s.Name.ToUpper() == dto.Name.ToUpper() &&
                        s.Id != shift.Id // exepto el que estamos actualizando
                );

                if (existsSameName)
                    return ResponseHelper.Fail<ShiftDto>(400,
                        "Ya existe un turno activo con este nombre en esta ubicación.");
            }

            // =======================================================
            // VALIDAR MISMA HORA (solo si cambia)
            // =======================================================
            if (dto.StartTime.HasValue && dto.StartTime.Value != shift.StartTime)
            {
                var existsSameStartTime = await _db.Shifts.AnyAsync(s =>
                        s.IsActive &&
                        s.LocationId == (dto.LocationId ?? shift.LocationId) &&
                        s.StartTime == dto.StartTime &&
                        s.Id != shift.Id // para no chocar con el mismo que estamos mapeando
                );

                if (existsSameStartTime)
                    return ResponseHelper.Fail<ShiftDto>(400,
                        "Ya existe un turno activo que inicia a la misma hora en esta ubicación.");
            }

            // =======================================================
            // 5. APLICAR CAMBIOS
            // =======================================================
            Guid userUpdate = _userContext.GetUserId();
            shift.ApplyUpdate(dto, userUpdate);

            // lo Movi al apli update directamente 
            //shift.UpdatedById = Guid.NewGuid(); // luego se reemplaza con usuario real
            //shift.UpdatedDate = DateTime.UtcNow;

            // =======================================================
            // GUARDAR CAMBIOS
            await _db.SaveChangesAsync();

            return ResponseHelper.Success(200, "Turno actualizado exitosamente.", shift.ToDto());
        }
        // por si no hay autorizacion
        catch (UnauthorizedAccessException ex)
        {
            return ResponseHelper.Fail<ShiftDto>(401, ex.Message);
        }
        catch (Exception ex)
        {
            return ResponseHelper.Fail<ShiftDto>(500, $"Error interno al crear el turno: {ex.Message}");
        }
    }


    public async Task<ResponseDto<bool>> DeleteShiftAsync(Guid id)
    {
        try
        {
            // =======================================================
            // BUSCAR TURNO
            // =======================================================
            var shift = await _db.Shifts
                .FirstOrDefaultAsync(s => s.Id == id && s.IsActive);

            if (shift == null)
            {
                return ResponseHelper.Fail<bool>(
                    404,
                    "El turno especificado no existe o ya se encuentra inactivo."
                );
            }

            // =======================================================
            // OBTENER USUARIO 
            // =======================================================
            Guid userId;
            try
            {
                userId = _userContext.GetUserId();
            }
            catch (UnauthorizedAccessException ex)
            {
                return ResponseHelper.Fail<bool>(401, ex.Message);
            }

            // =======================================================
            // SOFT DELETE
            // =======================================================
            shift.IsActive = false;
            shift.UpdatedById = userId;
            shift.UpdatedDate = DateTime.UtcNow;

            await _db.SaveChangesAsync();


            return ResponseHelper.Success(200, "Turno eliminado exitosamente.", true);
        }
        catch (Exception ex)
        {
            return ResponseHelper.Fail<bool>(
                500,
                $"Error interno al eliminar el turno: {ex.Message}"
            );
        }
    }


    public async Task<ResponseDto<ShiftDto>> GetShiftByIdAsync(Guid id)
    {
        try
        {
            // =======================================================
            // 1. BUSCAR EL TURNO EN SIGREF
            // =======================================================
            var shift = await _db.Shifts
                .FirstOrDefaultAsync(s => s.Id == id && s.IsActive);

            if (shift == null)
            {
                return ResponseHelper.Fail<ShiftDto>(
                    404,
                    "El turno especificado no existe o está inactivo."
                );
            }

            // =======================================================
            // CREAR DTO BASE
            // =======================================================
            var dto = shift.ToDto();

            // =======================================================
            // OBTENER EL NOMBRE DEL LOCATION DESDE FHIR
            // =======================================================
            try
            {
                // Solo pedimos lo necesario: id y name
                var location = await _fhirClient.ReadAsync<Hl7.Fhir.Model.Location>(
                    $"Location/{shift.LocationId}?_elements=id,name"
                );

                dto.NameLocation = location.Name; // FHIR: Location.Name
            }
            catch (FhirOperationException ex)
                when (ex.Status == System.Net.HttpStatusCode.NotFound)
            {
                // Si no existe en FHIR  devolvemos null
                dto.NameLocation = null;
            }

            return ResponseHelper.Success(
                200,
                "Turno obtenido exitosamente.",
                dto
            );
        }
        catch (Exception ex)
        {
            return ResponseHelper.Fail<ShiftDto>(
                500,
                $"Error interno al obtener el turno: {ex.Message}"
            );
        }
    }


    public async Task<ResponseDto<PagedResult<ShiftDto>>> GetFilteredShiftsAsync(ShiftFilterDto filter)
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
                // Si usa PostgreSQL y querés case-insensitive real:
                // query = query.Where(s => EF.Functions.ILike(s.Name, $"%{filter.Name.Trim()}%"));
            }

            if (filter.LocationId.HasValue)
                query = query.Where(s => s.LocationId == filter.LocationId.Value);

            // =======================================================
            // FILTRO POR LOCATION NAME (FHIR LOOKUP)
            // =======================================================
            if (!string.IsNullOrWhiteSpace(filter.LocationName))
            {
                var searchParams = new SearchParams()
                    .Where($"name:contains={filter.LocationName}")
                    .Select("_elements=id,name");

                var fhirResults = await _fhirClient.SearchAsync<FhirLocation>(searchParams);

                var fhirLocationIds = fhirResults.Entry
                    .Where(e => e.Resource is FhirLocation)
                    .Select(e => Guid.Parse(((FhirLocation)e.Resource).Id))
                    .ToList();

                if (fhirLocationIds.Count == 0)
                {
                    return ResponseHelper.Success(200, "No hay resultados.", new PagedResult<ShiftDto>
                    {
                        Items = new List<ShiftDto>(),
                        Pagination = new PaginationDto
                        {
                            CurrentPage = pageNumber,
                            PageSize = pageSize,
                            TotalItems = 0,
                            TotalPages = 0
                        }
                    });
                }

                query = query.Where(s => fhirLocationIds.Contains(s.LocationId));
            }

            // =======================================================
            // 4. TOTAL DE REGISTROS (DESPUÉS DE LOS FILTROS)
            // =======================================================
            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

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

            Dictionary<Guid, string?> locationNames = new();

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
                            loc => Guid.Parse(loc.Id),
                            loc => loc.Name
                        );
                }
                catch
                {
                    // En caso de error con FHIR devolvemos NameLocation = null
                }
            }

            // =======================================================
            // MAPEAR SHIFTS A DTO RECUPERANDO NOMBRE DEL NOMBRE LOCACION
            // =======================================================
            var dtoList = shifts.Select(shift =>
            {
                var dto = shift.ToDto();

                if(locationNames.TryGetValue(shift.LocationId, out var locName))
                    dto.NameLocation = locName;
                else
                    dto.NameLocation = null;

                return dto;
            }).ToList();
            
            var paged = new PagedResult<ShiftDto>
            {
                Items = dtoList,
                Pagination = new PaginationDto
                {
                    CurrentPage = pageNumber,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    TotalPages = totalPages
                }
            };

            return ResponseHelper.Success(200, "Listado de turnos obtenido.", paged);
        }
        catch (Exception ex)
        {
            return ResponseHelper.Fail<PagedResult<ShiftDto>>(500,
                $"Error interno al obtener los turnos: {ex.Message}");
        }
    }
}