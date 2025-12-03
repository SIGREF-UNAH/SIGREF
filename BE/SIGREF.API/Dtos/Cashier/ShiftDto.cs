namespace SIGREF.API.Dtos.Cashier;

public class ShiftDto
{
    public Guid Id { get; set; }

    public string LocationId { get; set; }

    public string Name { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }
    public bool IsActive { get; set; }
    
    public string? NameLocation { get; set; }
    
}