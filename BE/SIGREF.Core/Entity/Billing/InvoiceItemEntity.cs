using SIGREF.Core.Entity.Catalogs;
using SIGREF.Core.Entity.common;

namespace SIGREF.Core.Entity.Billing;

/// <summary>
/// Representa una línea de detalle dentro de una factura. 
/// Almacena un "snapshot" o captura del servicio al momento de la venta 
/// para garantizar la integridad histórica frente a cambios en el catálogo.
/// </summary>
public class InvoiceItemEntity : BaseEntity
{
    #region Relaciones (Foreign Keys)
    /// <summary> Identificador de la factura a la que pertenece este ítem. </summary>
    public Guid InvoiceId { get; set; }
    
    /// <summary> Referencia de navegación a la factura padre. </summary>
    public InvoiceEntity Invoice { get; set; } = null!;

    /// <summary> Identificador del servicio de salud original en el catálogo. </summary>
    public Guid ServiceId { get; set; }

    /// <summary> Referencia al servicio del catálogo (HealthService). </summary>
    public HealthService Service { get; set; } = null!;
    #endregion

    #region Datos Congelados (Snapshot Histórico)
    /// <summary> 
    /// Nombre del servicio capturado en el momento de la facturación. 
    /// Si el nombre del servicio cambia en el catálogo, este campo no se verá afectado.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary> Cantidad de unidades facturadas de este servicio. </summary>
    public int Quantity { get; set; }

    /// <summary> Precio unitario del servicio al momento de la venta. </summary>
    public decimal UnitPrice { get; set; }

    /// <summary> 
    /// Monto total de la línea (Quantity * UnitPrice). 
    /// Nota: Los descuentos comerciales se aplican a nivel de cabecera (InvoiceDiscount) 
    /// y no restan valor a este campo para mantener la trazabilidad bruta.
    /// </summary>
    public decimal TotalAmount { get; set; }
    #endregion
}