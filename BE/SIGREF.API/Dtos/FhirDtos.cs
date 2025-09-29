#nullable enable
namespace SIGREF.API.Dtos
{
    /* 
        ========== DTOs comunes usados en múltiples entidades FHIR ========== 
        En este apartado se colocarán los DTOs que son reutilizables en varias entidades FHIR.
    */

    public class ReferenceDto // Para relaciones entre recursos
    {
        public string? Identifier { get; set; }
        public string Reference { get; set; } = string.Empty;
        public string? Type { get; set; }
        public string? Display { get; set; }
    }

    public class AttachmentDto // Para archivos adjuntos como imágenes, documentos, etc.
    {
        public string? Url { get; set; }
        public string? ContentType { get; set; }
        public string? Title { get; set; }
    }

    public class EligibilityDto // Para elegibilidad
    {
        public CodeableConceptDto? Code { get; set; }
        public string? Comment { get; set; }
    }

    public class CodeableConceptDto // Para conceptos codificados
    {
        public List<CodingDto> Coding { get; set; } = new();
        public string? Text { get; set; }
    }

    public class CodingDto 
    {
        public string? System { get; set; }
        public string? Version { get; set; }
        public string? Code { get; set; }
        public string? Display { get; set; }
        public bool? UserSelected { get; set; }
    }

    public class AddressDto // Para direcciones
    {
        public string? Use { get; set; }
        public string? Type { get; set; }
        public string? Text { get; set; }
        public List<string> Line { get; set; } = new();
        public string? City { get; set; }
        public string? District { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }
    }

    public class ExtendedContactDetailDto // Para detalles de contacto
    {
        public CodeableConceptDto? Purpose { get; set; }
        public string? Name { get; set; }
        public ContactPointDto? Telecom { get; set; }
        public AddressDto? Address { get; set; }
        public ReferenceDto? Organization { get; set; }
        public PeriodDto? Period { get; set; }
    }

    public class ContactPointDto // Para puntos de contacto como teléfono, email, etc.
    {
        public string? System { get; set; } // phone, email, fax, etc.
        public string? Value { get; set; }
        public string? Use { get; set; } // home, work, mobile, etc.
        public int? Rank { get; set; }
    }

    public class AvailabilityDto // Para horarios de disponibilidad
    {
        public AvailableTimeDto? AvailableTime { get; set; }
        public NotAvailableTimeDto? NotAvailableTime { get; set; }
    }

    public class AvailableTimeDto 
    {
        public List<string>? DaysOfWeek { get; set; } // mon, tue, wed, thu, fri, sat, sun
        public bool? AllDay { get; set; }
        public TimeOnly? AvalaibleStartTime { get; set; }
        public TimeOnly? AvalaibleEndTime { get; set; }
    }

    public class NotAvailableTimeDto
    {
        public string? Description { get; set; }
        public PeriodDto? During { get; set; }
    }

    public class PeriodDto // Para períodos de tiempo
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }
}
