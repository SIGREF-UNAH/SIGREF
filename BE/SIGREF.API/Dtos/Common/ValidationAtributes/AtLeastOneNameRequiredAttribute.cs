#nullable enable
using System.ComponentModel.DataAnnotations;

namespace SIGREF.API.Dtos.Common.ValidationAtributes
{
    public class AtLeastOneNameRequiredAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is List<HumanNameDto> names && names.Any())
            {
                if (names.Any(n => !string.IsNullOrEmpty(n.Given?.FirstOrDefault()) || !string.IsNullOrEmpty(n.Family)))
                {
                    return ValidationResult.Success;
                }
            }
            return new ValidationResult("Al menos un nombre debe tener 'given' o 'family'.");
        }
    }
}
