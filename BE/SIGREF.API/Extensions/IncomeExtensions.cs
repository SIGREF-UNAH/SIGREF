#nullable enable
using Hl7.Fhir.Model;
using SIGREF.API.Dtos.Income;
using SIGREF.API.Extensions.Common;

namespace SIGREF.API.Extensions;

public static class IncomeExtensions
{
    private const string ServiceCodeSystem = "http://sigref.unah.edu.hn/fhir/CodeSystem/healthcare-services";
    private const string ReceiptNumberSystem = "http://sigref.unah.edu.hn/fhir/identifier/receipt-number";
    private const string CounterpartExtensionUrl = "http://sigref.unah.edu.hn/fhir/StructureDefinition/counterpart";
    private const string OriginalIncomeExtensionUrl = "http://sigref.unah.edu.hn/fhir/StructureDefinition/original-income";
    private const string CounterpartReasonExtensionUrl = "http://sigref.unah.edu.hn/fhir/StructureDefinition/counterpart-reason";

    /// <summary>
    /// Convierte un recurso FHIR ChargeItem a IncomeDto
    /// </summary>
    public static IncomeDto ToIncomeDto(this ChargeItem chargeItem)
    {
        var dto = new IncomeDto
        {
            Id = chargeItem.Id,
            Active = chargeItem.Status == ChargeItem.ChargeItemStatus.Billable || chargeItem.Status == ChargeItem.ChargeItemStatus.Billed,
            Identifier = chargeItem.Identifier?.Select(i => i.ToDto()).ToList(),
            LastUpdated = chargeItem.Meta?.LastUpdated?.DateTime
        };

        // Número de recibo desde identifier
        var receiptIdentifier = chargeItem.Identifier?.FirstOrDefault(i => i.System == ReceiptNumberSystem);
        dto.ReceiptNumber = receiptIdentifier?.Value ?? string.Empty;

        // Monto desde priceOverride
        if (chargeItem.PriceOverride != null)
        {
            dto.Amount = chargeItem.PriceOverride.Value ?? 0m;
        }

        // Fecha del ingreso
        if (chargeItem.Occurrence is FhirDateTime occurrenceDateTime)
        {
            dto.IncomeDate = occurrenceDateTime.ToDateTimeOffset(TimeSpan.Zero).DateTime;
        }
        else if (chargeItem.Occurrence is Period occurrencePeriod && occurrencePeriod.Start != null)
        {
            dto.IncomeDate = DateTime.Parse(occurrencePeriod.Start);
        }

        // Descripción desde el código del servicio
        dto.Description = chargeItem.Code?.Text ?? chargeItem.Code?.Coding?.FirstOrDefault()?.Display;

        // Paciente
        if (chargeItem.Subject != null)
        {
            dto.Patient = chargeItem.Subject.ToReferenceDto();
        }

        // Practitioner desde performer
        var practitionerPerformer = chargeItem.Performer?.FirstOrDefault();
        if (practitionerPerformer?.Actor != null)
        {
            dto.Practitioner = practitionerPerformer.Actor.ToReferenceDto();
        }

        // Healthcare service
        var healthcareService = chargeItem.Service?.FirstOrDefault();
        if (healthcareService != null)
        {
            dto.Healthcare = healthcareService.ToReferenceDto();
        }

        // Location desde context (si es un Location)
        if (chargeItem.Context != null && chargeItem.Context.Reference?.StartsWith("Location/") == true)
        {
            dto.Location = chargeItem.Context.ToReferenceDto();
        }

        // Organization
        if (chargeItem.PerformingOrganization != null)
        {
            dto.Organization = chargeItem.PerformingOrganization.ToReferenceDto();
        }

        // Extensiones para contrapartidas
        var counterpartExt = chargeItem.Extension?.FirstOrDefault(e => e.Url == CounterpartExtensionUrl);
        if (counterpartExt?.Value is FhirBoolean counterpartValue)
        {
            dto.IsCounterpart = counterpartValue.Value ?? false;
        }

        var originalIncomeExt = chargeItem.Extension?.FirstOrDefault(e => e.Url == OriginalIncomeExtensionUrl);
        if (originalIncomeExt?.Value is FhirString originalIncomeValue)
        {
            dto.CounterpartId = originalIncomeValue.Value;
        }

        return dto;
    }

    /// <summary>
    /// Convierte un CreateIncomeDto a un recurso FHIR ChargeItem
    /// </summary>
    public static ChargeItem ToFhirChargeItem(this CreateIncomeDto dto)
    {
        var chargeItem = new ChargeItem
        {
            Status = dto.Active ? ChargeItem.ChargeItemStatus.Billable : ChargeItem.ChargeItemStatus.NotBillable,
            Code = new CodeableConcept
            {
                Coding = new List<Coding>
                {
                    new Coding(ServiceCodeSystem, "income", "Ingreso por Servicio Médico")
                },
                Text = dto.Description ?? "Ingreso por servicio médico"
            },
            Subject = dto.Patient.ToFhirReference(),
            Occurrence = new FhirDateTime(dto.IncomeDate),
            Quantity = new Quantity { Value = 1 },
            PriceOverride = new Money
            {
                Value = dto.Amount
            },
            Identifier = new List<Identifier>
            {
                new Identifier
                {
                    System = ReceiptNumberSystem,
                    Value = dto.ReceiptNumber,
                    Use = Identifier.IdentifierUse.Official
                }
            },
            Extension = new List<Extension>
            {
                new Extension(CounterpartExtensionUrl, new FhirBoolean(false))
            },
            Meta = new Meta
            {
                LastUpdated = DateTimeOffset.Now,
                VersionId = "1"
            }
        };

        // Agregar identificadores adicionales si existen
        if (dto.Identifier != null && dto.Identifier.Any())
        {
            foreach (var identifier in dto.Identifier)
            {
                chargeItem.Identifier.Add(identifier.ToFhirIdentifier());
            }
        }

        // Agregar practitioner como performer
        if (dto.Practitioner != null)
        {
            chargeItem.Performer = new List<ChargeItem.PerformerComponent>
            {
                new ChargeItem.PerformerComponent
                {
                    Actor = dto.Practitioner.ToFhirReference(),
                    Function = new CodeableConcept
                    {
                        Text = "Practitioner"
                    }
                }
            };
        }

        // Agregar healthcare service
        if (dto.Healthcare != null)
        {
            chargeItem.Service = new List<ResourceReference>
            {
                dto.Healthcare.ToFhirReference()
            };
        }

        // Agregar location como context
        if (dto.Location != null)
        {
            chargeItem.Context = dto.Location.ToFhirReference();
        }

        // Agregar organization
        if (dto.Organization != null)
        {
            chargeItem.PerformingOrganization = dto.Organization.ToFhirReference();
        }

        return chargeItem;
    }

    /// <summary>
    /// Convierte un CreateCounterpartIncomeDto a un recurso FHIR ChargeItem
    /// </summary>
    public static ChargeItem ToFhirChargeItem(this CreateCounterpartIncomeDto dto)
    {
        // Crear el ChargeItem usando el método base
        var chargeItem = ((CreateIncomeDto)dto).ToFhirChargeItem();

        // Marcar como contrapartida (anulación)
        chargeItem.Status = ChargeItem.ChargeItemStatus.NotBillable;

        // Actualizar extensión de contrapartida
        var counterpartExt = chargeItem.Extension?.FirstOrDefault(e => e.Url == CounterpartExtensionUrl);
        if (counterpartExt != null)
        {
            counterpartExt.Value = new FhirBoolean(true);
        }
        else
        {
            chargeItem.Extension?.Add(new Extension(CounterpartExtensionUrl, new FhirBoolean(true)));
        }

        // Agregar referencia al ingreso original
        chargeItem.Extension?.Add(new Extension(OriginalIncomeExtensionUrl, new FhirString(dto.OriginalIncomeId)));

        // Agregar razón de la contrapartida
        chargeItem.Extension?.Add(new Extension(CounterpartReasonExtensionUrl, new FhirString(dto.Reason)));

        // Invertir el monto (contrapartida es negativa)
        if (chargeItem.PriceOverride != null)
        {
            chargeItem.PriceOverride.Value = -Math.Abs(chargeItem.PriceOverride.Value ?? 0);
        }

        // Actualizar descripción para indicar que es una anulación
        if (chargeItem.Code != null)
        {
            chargeItem.Code.Text = $"ANULACIÓN: {chargeItem.Code.Text} - Razón: {dto.Reason}";
        }

        return chargeItem;
    }
}