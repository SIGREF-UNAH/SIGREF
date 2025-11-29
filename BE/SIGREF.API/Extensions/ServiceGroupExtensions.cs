using Hl7.Fhir.Model;
using SIGREF.API.Dtos.ServiceGroup;
using SIGREF.API.Extensions.Common;
using FhirList = Hl7.Fhir.Model.List;

namespace SIGREF.API.Extensions
{
    public static class ServiceGroupExtensions
    {
        public static ServiceGroupDto ToDto(this FhirList list)
        {
            if (list == null) return new ServiceGroupDto();

            return new ServiceGroupDto
            {
                Id = list.Id,
                Status = list.Status?.ToString().ToLower(),
                Mode = list.Mode?.ToString().ToLower(),
                Title = list.Title,
                Code = list.Code?.ToCodeableConceptDto(),
                Date = list.DateElement?.ToDateTime()?.ToDateTimeOffset(TimeSpan.Zero).DateTime,
                // Items will be populated in the service as they need to be fetched
                Items = new List<Dtos.Healthcare.HealthcareDto>() 
            };
        }

        public static FhirList ToFhirList(this CreateServiceGroupDto dto)
        {
            var list = new FhirList
            {
                Title = dto.Title,
                Status = Enum.Parse<Hl7.Fhir.Model.List.ListStatus>(dto.Status, true),
                Mode = Enum.Parse<Hl7.Fhir.Model.ListMode>(dto.Mode, true),
                Code = dto.Code?.ToFhirCodeableConcept(),
                DateElement = new FhirDateTime(DateTimeOffset.Now),
                Entry = new List<FhirList.EntryComponent>()
            };

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

            return list;
        }

        public static void ApplyUpdate(this FhirList list, UpdateServiceGroupDto dto)
        {
            if (!string.IsNullOrEmpty(dto.Title)) list.Title = dto.Title;
            if (!string.IsNullOrEmpty(dto.Status)) list.Status = Enum.Parse<Hl7.Fhir.Model.List.ListStatus>(dto.Status, true);
            if (!string.IsNullOrEmpty(dto.Mode)) list.Mode = Enum.Parse<Hl7.Fhir.Model.ListMode>(dto.Mode, true);
            if (dto.Code != null) list.Code = dto.Code.ToFhirCodeableConcept();

            if (dto.HealthcareServiceIds != null)
            {
                list.Entry = new List<FhirList.EntryComponent>();
                foreach (var id in dto.HealthcareServiceIds)
                {
                    list.Entry.Add(new FhirList.EntryComponent
                    {
                        Item = new ResourceReference($"HealthcareService/{id}")
                    });
                }
            }
            
            list.DateElement = new FhirDateTime(DateTimeOffset.Now);
        }
    }
}
