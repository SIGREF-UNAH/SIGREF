
namespace SIGREF.Core.Entity.Dashboard;
public class DashboardFact
{
    // ===============================
    // IDENTIFICADORES PRINCIPALES
    // ===============================
    public Guid InvoiceId { get; set; }
    public Guid? ItemId { get; set; }

    // ===============================
    // DIMENSIÓN DE TIEMPO
    // ===============================
    public DateTime CreatedDate { get; set; }

    // ===============================
    // INGRESOS Y MONTO REAL (OPTIMIZADO)
    // ===============================
    public decimal FinalTotal { get; set; }

    /// <summary>
    /// Monto real considerando notas de crédito/débito
    /// (FinalTotal positivo o negativo según el tipo).
    /// </summary>
    public decimal RealIncome { get; set; }

    // ===============================
    // ESTADO Y TIPO DE FACTURA
    // ===============================
    public string InvoiceType { get; set; }
    public string Status { get; set; }

    // ===============================
    // DIMENSIÓN DE SHIFT (TURNO)
    // ===============================
    public Guid? ShiftId { get; set; }
    public string? ShiftName { get; set; }

    // ===============================
    // DIMENSIÓN DE LOCATION (CLAVE)
    // ===============================
    // Locations viene de FHIR SU ID ES STRING
    public string? LocationId { get; set; }
    //public string? LocationName { get; set; } // Recuperado en Bundle

    // ===============================
    // DIMENSIÓN DE SERVICIOS/PAQUETES
    // ===============================
    public Guid? ServiceId { get; set; }
    public string FhirServiceId { get; set; }
    //public string? ServiceName { get; set; }
    public string? PackageId { get; set; }
    //public string? PackageName { get; set; }

    // ===============================
    // DIMENSIÓN DE PACIENTE
    // ===============================
    public string? PatientIdFhir { get; set; }

    // ===============================
    // DIMENSIÓN DE ITEMS / SERVICIOS TOTALES
    // ===============================
    public int TotalItems { get; set; } // 1 por item, 0 si es invoice sin item (credit/debit)

    // ===============================
    // DIMENSIÓN CASHIER (OPCIONAL PERO ÚTIL)
    // ===============================
    public Guid? CashierSessionId { get; set; }
    //public string? CashierUserName { get; set; }
}

