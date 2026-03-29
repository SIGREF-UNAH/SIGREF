namespace SIGREF.API.Dtos.Invoice;

public class InvoiceItemCreateDto
{
    public string ServiceId { get; set; } = null!;

    /// <summary>
    /// Nombre del servicio tal como lo ve el usuario en pantalla.
    /// Se congela en el historial de la factura.
    /// </summary>
    public string NameService { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    // TotalAmount se elimina del DTO.
    // El servidor lo calcula como: Quantity * UnitPrice
    // Así evitamos que el cliente manipule totales.
}