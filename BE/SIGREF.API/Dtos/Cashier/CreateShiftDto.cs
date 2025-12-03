using System.ComponentModel.DataAnnotations;

namespace SIGREF.API.Dtos.Cashier;

public class CreateShiftDto
{
    [Required(ErrorMessage = "El ID de la ubicación (LocationId) es obligatorio.")]
    public string LocationId { get; set; }

    [Required(ErrorMessage = "El nombre del turno es obligatorio.")]
    [StringLength(150, ErrorMessage = "El nombre no puede superar los 150 caracteres.")]
    public string Name { get; set; }

    [Required(ErrorMessage = "La hora de inicio es obligatoria.")]
    public TimeOnly StartTime { get; set; }

    [Required(ErrorMessage = "La hora de finalización es obligatoria.")]
    public TimeOnly EndTime { get; set; }
    
    public bool IsActive { get; set; } = true;
}