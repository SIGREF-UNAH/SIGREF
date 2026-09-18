using System.ComponentModel.DataAnnotations;

namespace SIGREF.API.Dtos.Cashier;

public class UpdateShiftDto : SIGREF.Common.Dtos.UpdateRequestDto
{
    public string LocationId { get; set; }

    [StringLength(150, ErrorMessage = "El nombre no puede superar los 150 caracteres.")]
    public string? Name { get; set; }

    public TimeOnly? StartTime { get; set; }

    public TimeOnly? EndTime { get; set; }
    public bool? IsActive { get; set; }
}
