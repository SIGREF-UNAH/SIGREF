namespace SIGREF.API.Dtos.Report;

/// <summary>
/// Información institucional del hospital para branding/encabezado.
/// </summary>
public class HospitalInfoDto
{
    public string HospitalName { get; set; } = string.Empty;
    public string DirectorName { get; set; } = string.Empty;

    /// <summary>
    /// Código interno/externo del hospital (si aplica).
    /// </summary>
    public string HospitalCode { get; set; } = string.Empty;

    /// <summary>
    /// Identificador del recurso de imagen (ej: en storage) para el logo del hospital.
    /// </summary>
    public Guid HospitalLogoImageId { get; set; }

    /// <summary>
    /// Identificador del recurso de imagen (ej: en storage) para el logo de Salud / Secretaría.
    /// </summary>
    public Guid HealthDepartmentLogoImageId { get; set; }

    public HospitalContactDto Contact { get; set; } = new();
}

/// <summary>
/// Datos de contacto del hospital.
/// </summary>
public class HospitalContactDto
{
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
}
