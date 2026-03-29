using SIGREF.Core.Entity.common;

namespace SIGREF.Core.Entity.Administration;

public class HospitalPropertiesEntity : BaseEntity
{
    // ===============================
    //        DATOS GENERALES
    // ===============================
    public string Name { get; set; } = null!;
    public string? Director { get; set; }
    public string? Subdirector { get; set; }
    public string? Location { get; set; }

    // ===============================
    //       IMEGES / LOGOS
    // ===============================
    public Guid? LogoMediaId { get; set; }

    // URL publica construida
    public string? UrlLogo { get; set; }
    public Guid? HealthLogoMediaId { get; set; }
    public string? UrlLogoHealth { get; set; }

    // ===============================
    //       CONTACTO
    // ===============================
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }

    // ===============================
    //       DATOS ADICIONALES
    // ===============================
    public string? HospitalCode { get; set; }
    public string? RTN { get; set; }
    public string? Website { get; set; }
    public string Currency { get; set; } = "LPS";

    // TODO : EL DIA QUE SE MANEJEN 2 TIPOS DE MONEDAS SERA NECESARIO ESTAR GUARDANDO CON EL TIPO DE CAMBIO UTILIZADO
    //[StringLength(10)]
    //[Column("exchange_version")]
    //public string? ExchangeVersion { get; set; }

    // ===============================
    //      CONTROL DEL SISTEMA
    // ===============================
    public bool IsSingleton { get; set; } = true;
}