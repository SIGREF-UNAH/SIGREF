using Microsoft.EntityFrameworkCore;
using SIGREF.API.Database;
using SIGREF.API.Database.Entity.Catalogs;
using SIGREF.API.Dtos.Common;
using SIGREF.API.Dtos.Healthcare;
using SIGREF.API.Extensions;

namespace SIGREF.API.Services.Healthcare;

/// <summary>
/// Implementación del servicio de aplicación para Healthcare.
/// Contiene la lógica de negocio y la orquestación entre FHIR y SIGREF.
/// </summary>
public class HealthcareApplicationService : IHealthcareService
{
    private readonly HealthcareFHIRService _fhirService;
    private readonly SIGREFContext _dbSigref;

    public HealthcareApplicationService(
        HealthcareFHIRService fhirService,
        SIGREFContext db)
    {
        _fhirService = fhirService;
        _dbSigref = db;
    }

    private async Task EnrichWithCostAsync(List<HealthcareDto> items)
    {
        var fhirIds = items
            .Where(x => !string.IsNullOrEmpty(x.Id))
            .Select(x => x.Id!)
            .ToList();

        if (fhirIds.Count == 0)
            return;

        var prices = await _dbSigref.HealthServices
            .Where(x => fhirIds.Contains(x.HealthServiceFhirId))
            .ToDictionaryAsync(
                x => x.HealthServiceFhirId,
                x => x.Price);

        foreach (var item in items)
        {
            if (item.Id != null && prices.TryGetValue(item.Id, out var price))
            {
                item.Cost = price;
            }
        }
    }


    // ============================================================
    //                     LISTAR (PAGINADO)
    // ============================================================
    public async Task<ResponseDto<PagedResultDto<HealthcareDto>>> GetFilteredAsync(
        HealthcareFilterDto filter)
    {
        // Obtener datos desde FHIR
        var fhirResult = await _fhirService.GetFilteredHealthcaresAsync(filter);

        // Mapear FHIR DTO
        var items = fhirResult.Items
            .Select(h => h.ToDto())
            .ToList();

        // Meter costo solo si se solicita
        if (filter.IncludeCost && items.Count > 0)
        {
            await EnrichWithCostAsync(items);
        }

        //  Devolver resultado
        return new ResponseDto<PagedResultDto<HealthcareDto>>
        {
            Data = new PagedResultDto<HealthcareDto>
            {
                Items = items,
                Pagination = fhirResult.Pagination
            },
            Status = true,
            StatusCode = StatusCodes.Status200OK
        };
    }


    // ============================================================
    //                    OBTENER POR ID
    // ============================================================
    public async Task<ResponseDto<HealthcareDto?>> GetByIdAsync(string id)
    {
        var healthcare = await _fhirService.GetHealthcareByIdAsync(id);

        if (healthcare == null)
        {
            return new ResponseDto<HealthcareDto?>
            {
                Data = null,
                Status = false,
                StatusCode = StatusCodes.Status404NotFound,
                Message = $"HealthcareService con id '{id}' no encontrado"
            };
        }

        // Mapear FHIR a DTO
        var resultDto = healthcare.ToDto();

        // Consultar el costo desde la base de datos SIGREF
        var healthServiceEntity = await _dbSigref.HealthServices
            .FirstOrDefaultAsync(x => x.HealthServiceFhirId == id);

        // Si existe la entidad, asignar el costo al DTO
        if (healthServiceEntity != null)
        {
            resultDto.Cost = healthServiceEntity.Price;
        }
        else
        {
            // Opcional: Si no existe, puedes asignar null o un valor por defecto
            resultDto.Cost = null;
        }

        return new ResponseDto<HealthcareDto?>
        {
            Data = resultDto,
            Status = true,
            StatusCode = StatusCodes.Status200OK
        };
    }

    // ============================================================
    //                         CREAR
    // ============================================================
    public async Task<ResponseDto<HealthcareDto>> CreateAsync(CreateHealthcareDto dto)
    {
        // Crear recurso FHIR
        var fhirHealthcare = dto.ToFhirHealthcare();
        var created = await _fhirService.CreateHealthcareAsync(fhirHealthcare);

        // Guardar costo en SIGREF
        var entity = new HealthService
        {
            HealthServiceFhirId = created.Id,
            Price = dto.Cost!.Value
        };

        _dbSigref.HealthServices.Add(entity);
        await _dbSigref.SaveChangesAsync();

        // Mapear a DTO
        var resultDto = created.ToDto();

        // 
        resultDto.Cost = entity.Price;

        return new ResponseDto<HealthcareDto>
        {
            Data = resultDto,
            Status = true,
            StatusCode = StatusCodes.Status201Created,
            Message = "Servicio medico creado correctamente"
        };
    }


    // ============================================================
    //                        ACTUALIZAR
    // ============================================================
    public async Task<ResponseDto<HealthcareDto>> UpdateAsync(
        string id,
        UpdateHealthcareDto dto)
    {
        // Obtener recurso FHIR existente
        var existing = await _fhirService.GetHealthcareByIdAsync(id);

        if (existing == null)
        {
            return new ResponseDto<HealthcareDto>
            {
                Status = false,
                StatusCode = StatusCodes.Status404NotFound,
                Message = $"HealthcareService con id '{id}' no encontrado"
            };
        }

        // Aplicar cambios FHIR
        existing.ApplyUpdate(dto);
        var updated = await _fhirService.UpdateHealthcareAsync(existing);

        // Actualizar costo en SIGREF (si aplica)
        var entity = await _dbSigref.HealthServices
            .FirstOrDefaultAsync(x => x.HealthServiceFhirId == id);

        if (entity != null && dto.Cost.HasValue)
        {
            entity.Price = dto.Cost.Value;
            await _dbSigref.SaveChangesAsync();
        }

        // Mapear FHIR  DTO
        var resultDto = updated.ToDto();

        //  Enriquecer con costo
        // - si vino en el DTO, usarlo
        // - si no vino, usar el valor persistido
        if (entity != null)
        {
            resultDto.Cost = entity.Price;
        }

        return new ResponseDto<HealthcareDto>
        {
            Data = resultDto,
            Status = true,
            StatusCode = StatusCodes.Status200OK,
            Message = "Servicio medico actualizado correctamente"
        };
    }


    // ============================================================
    //                        ELIMINAR
    // ============================================================
    public async Task<ResponseDto<bool>> DeleteAsync(string id)
    {
        // Verificar existencia en FHIR
        var existing = await _fhirService.GetHealthcareByIdAsync(id);

        if (existing == null)
        {
            return new ResponseDto<bool>
            {
                Data = false,
                Status = false,
                StatusCode = StatusCodes.Status404NotFound,
                Message = $"HealthcareService con id '{id}' no encontrado"
            };
        }

        //Eliminar recurso FHIR (fuente principal)
        await _fhirService.DeleteHealthcareAsync(id);

        // Eliminar datos asociados en SIGREF 
        try
        {
            var entity = await _dbSigref.HealthServices
                .FirstOrDefaultAsync(x => x.HealthServiceFhirId == id);

            if (entity != null)
            {
                _dbSigref.HealthServices.Remove(entity);
                await _dbSigref.SaveChangesAsync();
            }
        }
        catch
        {
            // No se revierte el delete FHIR
            // El sistema queda consistente a nivel funcional
            // Se podria loguear si se desea
        }

        // Respuesta final
        return new ResponseDto<bool>
        {
            Data = true,
            Status = true,
            StatusCode = StatusCodes.Status200OK,
            Message = "Servicio medico eliminado correctamente"
        };
    }

}