using System.ComponentModel;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Microsoft.EntityFrameworkCore;
using SIGREF.API.Database;
using SIGREF.API.Dtos.Cashier;
using SIGREF.API.Dtos.Common;
using SIGREF.API.Extensions;
using SIGREF.API.Helpers;
using SIGREF.Common.Dtos;
using SIGREF.Common.Helpers;
using SIGREF.Core.Entity.Cashier;
using SIGREF.Infrastructure.Keycloak.Interfaces;
using SIGREF.Infrastructure.Keycloak.Services.Auth;
using SIGREF.Infrastructure.Persistence;
using FhirLocation = Hl7.Fhir.Model.Location;

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
            Bundle results = await _fhirClient.SearchByIdAsync<FhirLocation>(dto.LocationId);

            if (results == null)
            {
                ResponseHelper.Fail<ShiftDto>(400, "La ubicación especificada no existe en el servidor FHIR.");
            }
            
           // try
           //{
           //    await _fhirClient.ReadAsync<Hl7.Fhir.Model.Location>(
           //        $"Location/{dto.LocationId}?_elements=id"
           //    );
           //}
           // catch (FhirOperationException ex)
           //    when (ex.Status == System.Net.HttpStatusCode.NotFound)
           //{
           //    return ResponseHelper.Fail<ShiftDto>(400, "La ubicación especificada no existe en el servidor FHIR.");
           // //}

            // try
            //{
            //    await _fhirClient.ReadAsync<Hl7.Fhir.Model.Location>(
            //        $"Location/{dto.LocationId}?_elements=id"
            //    );
            //}
            // catch (FhirOperationException ex)
            //    when (ex.Status == System.Net.HttpStatusCode.NotFound)
            //{
            //    return ResponseHelper.Fail<ShiftDto>(400, "La ubicación especificada no existe en el servidor FHIR.");
            // //}

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
            return ResponseHelper.Fail<ShiftDto>(500, $"Error interno al crear el turno: {ex.Message} | {ex.StackTrace} | {ex.InnerException?.Message}");
        }
    }

    public async Task<ResponseDto<ShiftDto>> UpdateShiftAsync(Guid id, UpdateShiftDto dto)
    {
        try
        {
            // 1. BUSCAR EL TURNO SIN IMPORTAR SI ESTÁ ACTIVO O NO
            var shift = await _db.Shifts.FirstOrDefaultAsync(s => s.Id == id);

            if (shift == null)
                return ResponseHelper.Fail<ShiftDto>(404, "El turno especificado no existe.");

            // 2. VALIDAR LOCATION SI SE ESTÁ CAMBIANDO
            if (!string.IsNullOrWhiteSpace(dto.LocationId) && dto.LocationId != shift.LocationId)
            {
                var results = await _fhirClient.SearchByIdAsync<FhirLocation>(dto.LocationId);
                if (results?.Entry?.Any() != true)
                    return ResponseHelper.Fail<ShiftDto>(400, "La ubicación especificada no existe en el servidor FHIR.");
            }

            var targetLocationId = dto.LocationId ?? shift.LocationId;

            // 3. VALIDAR NOMBRE DUPLICADO (solo si cambia y solo entre turnos ACTIVOS)
            if (!string.IsNullOrWhiteSpace(dto.Name) &&
                dto.Name.Trim().ToUpper() != shift.Name.Trim().ToUpper())
            {
                bool nameExists = await _db.Shifts.AnyAsync(s =>
                    s.Id != shift.Id &&
                    s.IsActive && // ← solo choca con turnos ACTIVOS
                    s.LocationId == targetLocationId &&
                    s.Name.Trim().ToUpper() == dto.Name.Trim().ToUpper());

                if (nameExists)
                    return ResponseHelper.Fail<ShiftDto>(400,
                        "Ya existe un turno activo con este nombre en esta ubicación.");
            }

            // 4. VALIDAR HORA DE INICIO DUPLICADA (solo entre turnos ACTIVOS)
            if (dto.StartTime.HasValue && dto.StartTime.Value != shift.StartTime)
            {
                bool timeExists = await _db.Shifts.AnyAsync(s =>
                    s.Id != shift.Id &&
                    s.IsActive && // ← solo con turnos activos
                    s.LocationId == targetLocationId &&
                    s.StartTime == dto.StartTime.Value);

                if (timeExists)
                    return ResponseHelper.Fail<ShiftDto>(400,
                        "Ya existe un turno activo que inicia a la misma hora en esta ubicación.");
            }

            // 5. APLICAR LOS CAMBIOS
            Guid userId = _userContext.GetUserId();

            shift.ApplyUpdate(dto, userId);

            // Permitir reactivar o desactivar explícitamente
            if (dto.IsActive.HasValue)
            {
                shift.IsActive = dto.IsActive.Value;
            }

            // Siempre actualizamos auditoría al modificar
            shift.UpdatedById = userId;
            shift.UpdatedDate = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return ResponseHelper.Success(200, "Turno actualizado exitosamente.", shift.ToDto());
        }
        catch (UnauthorizedAccessException ex)
        {
            return ResponseHelper.Fail<ShiftDto>(401, ex.Message);
        }
        catch (Exception ex)
        {
            return ResponseHelper.Fail<ShiftDto>(500, $"Error interno al actualizar el turno: {ex.Message}");
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


    public async Task<ResponseDto<PagedResultDto<ShiftDto>>> GetFilteredShiftsAsync(ShiftFilterDto filter)
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

                var fhirResults = await _fhirClient.SearchAsync<FhirLocation>(searchParams);

                var fhirLocationIds = fhirResults.Entry
                    .Where(e => e.Resource is FhirLocation)
                    .Select(e => ((FhirLocation)e.Resource).Id)
                    .ToList();


                if (fhirLocationIds.Count == 0)
                {
                    return ResponseHelper.Success(200, "No hay resultados.", new PagedResultDto<ShiftDto>
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
                            loc => loc.Id,   // string key
                            loc => loc.Name  // display name
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

                if (locationNames.TryGetValue(shift.LocationId, out var locName))
                    dto.NameLocation = locName;
                else
                    dto.NameLocation = null;

                return dto;
            }).ToList();

            var paged = new PagedResultDto<ShiftDto>
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
            return ResponseHelper.Fail<PagedResultDto<ShiftDto>>(500,
                $"Error interno al obtener los turnos: {ex.Message}");
        }
    }
}