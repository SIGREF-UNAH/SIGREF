namespace SIGREF.API.Dtos.Invoice;

public class InvoiceItemCreateDto
{
    public string ServiceId { get; set; }

    // el nombre del Servicio Ofrecido
    public string NameService { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal? Discount { get; set; }

    public decimal TotalAmount { get; set; }
    

}