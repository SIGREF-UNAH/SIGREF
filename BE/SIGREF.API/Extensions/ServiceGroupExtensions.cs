using Hl7.Fhir.Model;
using SIGREF.API.Dtos.Common;
using SIGREF.API.Dtos.ServiceGroup;
using SIGREF.API.Extensions.Common;
using FhirList = Hl7.Fhir.Model.List;

namespace SIGREF.API.Extensions;

public static class ServiceGroupExtensions
{
    public static ServiceGroupDto ToDto(this FhirList list)
    {
        if (list == null) return new ServiceGroupDto();

        return new ServiceGroupDto
        {
            Id = list.Id,
            Status = list.Status,
            Title = list.Title,
            Code = list.Code?.ToCodeableConceptDto(),
            Date = list.DateElement?.ToDateTime()?.ToDateTimeOffset(TimeSpan.Zero).DateTime,
            // Items serán poblados en el service (ya no aquí)
            HealthcareService = [],
            Locations = [],
            TotalPrice = 0  // Se calculará en el service
        };
    }

    /// <summary>
    /// Convierte HealthcareService a DTO simplificado para ServiceGroup
    /// </summary>
    public static ServiceGroupHealthcareDto ToSimplifiedDto(this HealthcareService service)
    {
        return new ServiceGroupHealthcareDto
        {
            Id = service.Id,
            Name = service.Name ?? string.Empty,
            Price = null  // Se llenará desde Postgres en el Service
        };
    }

    /// <summary>
    /// Convierte Location a DTO simplificado para ServiceGroup
    /// </summary>
    public static ServiceGroupLocationDto ToSimplifiedDto(this Location location)
    {
        return new ServiceGroupLocationDto
        {
            Id = location.Id,
            Name = location.Name ?? string.Empty
        };
    }

    public static FhirList ToFhirList(this CreateServiceGroupDto dto)
    {
        var list = new FhirList
        {
            Title = dto.Title,
            Status = dto.Status,
            //Significa que es una lista editable
            Mode = ListMode.Working,
            Code = dto.Code?.ToFhirCodeableConcept(),
            DateElement = new FhirDateTime(DateTime.Now),
            Entry = new List<FhirList.EntryComponent>(),
            Note = [new Annotation(){Text =  dto.Description}],
         };

        // Agregar servicios de salud como entries
        if (dto.HealthcareServiceIds != null)
        {
            foreach (var id in dto.HealthcareServiceIds)
            {
                list.Entry.Add(new FhirList.EntryComponent
                {
                    Item = new ResourceReference($"{nameof(HealthcareService)}/{id}")
                });
            }
        }

        // Agregar ubicaciones como entries (no extensions)
        if (dto.LocationIds != null && dto.LocationIds.Any())
        {
            foreach (var locationId in dto.LocationIds)
            {
                list.Entry.Add(new FhirList.EntryComponent
                {
                    Item = new ResourceReference($"{nameof(Location)}/{locationId}")
                });
            }
        }

        return list;
    }

    public static void ApplyUpdate(this FhirList list, UpdateServiceGroupDto dto)
    {
        if (!string.IsNullOrEmpty(dto.Title)) list.Title = dto.Title;
        if (dto.Status != list.Status) list.Status = dto.Status;
        if (dto.Code != null) list.Code = dto.Code.ToFhirCodeableConcept();
        if (!string.IsNullOrEmpty(dto.Description)) list.Note = [new Annotation(){Text = dto.Description}];

        // Actualizar servicios de salud y ubicaciones como entries
        if (dto.HealthcareServiceIds != null || dto.LocationIds != null)
        {
            list.Entry = [];

            // Agregar servicios de salud
            if (dto.HealthcareServiceIds != null)
            {
                foreach (var id in dto.HealthcareServiceIds)
                {
                    list.Entry.Add(new FhirList.EntryComponent
                    {
                        Item = new ResourceReference($"HealthcareService/{id}")
                    });
                }
            }

            // Agregar ubicaciones
            if (dto.LocationIds != null)
            {
                foreach (var locationId in dto.LocationIds)
                {
                    list.Entry.Add(new FhirList.EntryComponent
                    {
                        Item = new ResourceReference($"Location/{locationId}")
                    });
                }
            }
        }

        list.DateElement = new FhirDateTime(DateTimeOffset.Now);
    }

    public record FhirListWithIncludes
    {
        public FhirList List { get; set; }
        public List<Resource> IncludedResources { get; set; }
    }
}