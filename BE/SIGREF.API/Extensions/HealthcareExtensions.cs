using Hl7.Fhir.Model;
using SIGREF.API.Dtos.Common;
using SIGREF.API.Dtos.Healthcare;

namespace SIGREF.API.Extensions
{
    public static class HealthcareExtensions
    {
        // URLs para los campos adicionales
        private const string AbbreviationExtensionUrl = "http://sigref.api/extensions/healthcare/abbreviation";
        private const string CostExtensionUrl = "http://sigref.api/extensions/healthcare/cost";

        // Convertir FHIR HealthcareService a HealthcareDto
        public static HealthcareDto ToDto(this HealthcareService healthcare)
        {
            // Respuesta por si healthcare es null
            if (healthcare == null)
            {
                return new HealthcareDto
                {
                    Id = string.Empty,
                    Active = true,
                    Name = string.Empty,
                    Abbreviation = string.Empty,
                    Comment = string.Empty,
                    Cost = 0,
                    Specialty = new List<CodeableConceptDto>(),
                    ProvidedBy = new ReferenceDto(),
                    Location = new List<ReferenceDto>(),
                    LastUpdated = null
                };
            }

            // Obtener abbreviation y cost de extensiones
            var abbreviation = healthcare.GetStringExtension(AbbreviationExtensionUrl);
            var cost = healthcare.GetDecimalExtension(CostExtensionUrl) ?? 0;

            return new HealthcareDto
            {
                Id = healthcare.Id ?? string.Empty,
                Active = healthcare.Active ?? true,
                Name = healthcare.Name ?? string.Empty,
                Abbreviation = abbreviation ?? string.Empty,
                Comment = healthcare.Comment ?? string.Empty,
                Cost = cost,
                Specialty = healthcare.Specialty?.Select(s => new CodeableConceptDto
                {
                    Text = s?.Text ?? string.Empty,
                    Coding = s?.Coding?.Select(c => new CodingDto
                    {
                        System = c?.System ?? string.Empty,
                        Code = c?.Code ?? string.Empty,
                        Display = c?.Display ?? string.Empty,
                    }).ToList() ?? new List<CodingDto>(),
                }).ToList() ?? new List<CodeableConceptDto>(),
                ProvidedBy = healthcare.ProvidedBy == null ? null : new ReferenceDto
                {
                    Identifier = healthcare.ProvidedBy.Identifier?.Value,
                    Reference = healthcare.ProvidedBy.Reference ?? string.Empty,
                    Display = healthcare.ProvidedBy.Display ?? string.Empty,
                    Type = healthcare.ProvidedBy.Type,
                },
                Location = healthcare.Location?.Select(loc => new ReferenceDto
                {
                    Reference = loc?.Reference ?? string.Empty,
                    Display = loc?.Display ?? string.Empty,
                }).ToList() ?? new List<ReferenceDto>(),
                LastUpdated = healthcare.Meta?.LastUpdated?.DateTime,
            };
        }

        // Convertir CreateHealthcareDto a FHIR HealthcareService
        public static HealthcareService ToFhirHealthcare(this CreateHealthcareDto createDto)
        {
            if (createDto == null)
                throw new ArgumentNullException(nameof(createDto));

            var healthcare = new HealthcareService
            {
                Active = createDto.Active,
                Name = createDto.Name ?? string.Empty,
                Comment = createDto.Comment ?? string.Empty,
                Specialty = createDto.Specialty?.Select(s => new CodeableConcept
                {
                    Text = s?.Text ?? string.Empty,
                    Coding = s?.Coding?.Select(c => new Coding
                    {
                        System = c?.System ?? string.Empty,
                        Code = c?.Code ?? string.Empty,
                        Display = c?.Display ?? string.Empty,
                    }).ToList() ?? new List<Coding>(),
                }).ToList() ?? new List<CodeableConcept>(),
                ProvidedBy = createDto.ProvidedBy == null ? null : new ResourceReference
                {
                    Identifier = createDto.ProvidedBy.Identifier == null ? null : new Identifier
                    {
                        Value = createDto.ProvidedBy.Identifier
                    },
                    Reference = createDto.ProvidedBy.Reference ?? string.Empty, // Ej: "Organization/{id}"
                    Display = createDto.ProvidedBy.Display ?? string.Empty,
                    Type = createDto.ProvidedBy.Type,
                },
                Location = createDto.Location?.Select(loc => new ResourceReference
                {
                    Reference = loc?.Reference ?? string.Empty, // Ej: "Location/{id}"
                    Display = loc?.Display ?? string.Empty,
                }).ToList() ?? new List<ResourceReference>(),
                Meta = new Meta
                {
                    LastUpdated = DateTimeOffset.Now,
                    VersionId = "1"
                }
            };

            // Agregar extensiones personalizadas solo si tienen valor
            if (!string.IsNullOrEmpty(createDto.Abbreviation))
            {
                healthcare.AddExtension(AbbreviationExtensionUrl, new FhirString(createDto.Abbreviation));
            }

            healthcare.AddExtension(CostExtensionUrl, new FhirDecimal(createDto.Cost));

            return healthcare;
        }

        // Convertir UpdateHealthcareDto a FHIR HealthcareService
        public static HealthcareService ApplyUpdate(this HealthcareService existingHealthcare, UpdateHealthcareDto updateDto)
        {
            if (existingHealthcare == null)
                throw new ArgumentNullException(nameof(existingHealthcare));

            if (updateDto == null)
                return existingHealthcare;

            if (updateDto.Active != true || updateDto.Active != false)
                existingHealthcare.Active = updateDto.Active;

            if (!string.IsNullOrEmpty(updateDto.Name))
                existingHealthcare.Name = updateDto.Name;

            if (!string.IsNullOrEmpty(updateDto.Comment))
                existingHealthcare.Comment = updateDto.Comment;

            if (updateDto.Specialty != null)
            {
                existingHealthcare.Specialty = updateDto.Specialty.Select(s => new CodeableConcept
                {
                    Text = s?.Text ?? string.Empty,
                    Coding = s?.Coding?.Select(c => new Coding
                    {
                        System = c?.System ?? string.Empty,
                        Code = c?.Code ?? string.Empty,
                        Display = c?.Display ?? string.Empty,
                    }).ToList() ?? new List<Coding>(),
                }).ToList();
            }

            if (updateDto.ProvidedBy != null)
            {
                existingHealthcare.ProvidedBy = new ResourceReference
                {
                    Identifier = updateDto.ProvidedBy.Identifier == null ? null : new Identifier
                    {
                        Value = updateDto.ProvidedBy.Identifier
                    },
                    Reference = updateDto.ProvidedBy.Reference ?? string.Empty,
                    Display = updateDto.ProvidedBy.Display ?? string.Empty,
                    Type = updateDto.ProvidedBy.Type,
                };
            }

            if (updateDto.Location != null)
            {
                existingHealthcare.Location = updateDto.Location.Select(loc => new ResourceReference
                {
                    Reference = loc?.Reference ?? string.Empty,
                    Display = loc?.Display ?? string.Empty,
                }).ToList();
            }

            // Asegurarse de que las extensiones existen
            if (existingHealthcare.Extension == null)
                existingHealthcare.Extension = new List<Extension>();

            // Actualizar o crear extensión de Abbreviation
            if (updateDto.Abbreviation != null)
            {
                var abbreviationExtension = existingHealthcare.Extension.FirstOrDefault(e => e.Url == AbbreviationExtensionUrl);
                if (abbreviationExtension != null)
                {
                    abbreviationExtension.Value = new FhirString(updateDto.Abbreviation);
                }
                else
                {
                    existingHealthcare.Extension.Add(new Extension(AbbreviationExtensionUrl, new FhirString(updateDto.Abbreviation)));
                }
            }

            // Actualizar o crear extensión de Cost
            if (updateDto.Cost.HasValue)
            {
                var costExtension = existingHealthcare.Extension.FirstOrDefault(e => e.Url == CostExtensionUrl);
                if (costExtension != null)
                {
                    costExtension.Value = new FhirDecimal(updateDto.Cost.Value);
                }
                else
                {
                    existingHealthcare.Extension.Add(new Extension(CostExtensionUrl, new FhirDecimal(updateDto.Cost.Value)));
                }
            }

            // Actualizar metadatos
            if (existingHealthcare.Meta == null)
                existingHealthcare.Meta = new Meta();

            existingHealthcare.Meta.LastUpdated = DateTimeOffset.Now;

            // Incrementar versión
            if (int.TryParse(existingHealthcare.Meta.VersionId, out var currentVersion))
                existingHealthcare.Meta.VersionId = (currentVersion + 1).ToString();
            else
                existingHealthcare.Meta.VersionId = "2"; // Empezar desde 2 si no hay versión

            return existingHealthcare;
        }

        // helper methods para manejar extensiones
        private static string GetStringExtension(this HealthcareService healthcare, string url)
        {
            // Verificar si healthcare es null
            if (healthcare == null)
                return null;

            // Verificar si Extension es null o vacía
            if (healthcare.Extension == null || !healthcare.Extension.Any())
                return null;

            var extension = healthcare.Extension.FirstOrDefault(e => e?.Url == url);
            return (extension?.Value as FhirString)?.Value;
        }

        private static decimal? GetDecimalExtension(this HealthcareService healthcare, string url)
        {
            // Verificar si healthcare es null
            if (healthcare == null)
                return null;

            // Verificar si Extension es null o vacía
            if (healthcare.Extension == null || !healthcare.Extension.Any())
                return null;

            var extension = healthcare.Extension.FirstOrDefault(e => e?.Url == url);
            return (extension?.Value as FhirDecimal)?.Value;
        }

        private static void AddExtension(this HealthcareService healthcare, string url, DataType value)
        {
            if (healthcare == null)
                return;

            if (healthcare.Extension == null)
                healthcare.Extension = new List<Extension>();

            healthcare.Extension.Add(new Extension(url, value));
        }
    }
}