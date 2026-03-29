
using SIGREF.Core.Entity.common;

namespace SIGREF.Core.Entity.Billing;

public class InvoiceSerieEntity : BaseEntity
{
    // ===============================
    //         DATOS PRINCIPALES
    // ===============================
    public string Name { get; set; } = null!;

    public string Prefix { get; set; } = null!;

    // ===============================
    //       RANGO DE NUMERACIÓN

    public long StartNumber { get; set; }


    public long EndNumber { get; set; }

    public long CurrentNumber { get; set; }
    
}