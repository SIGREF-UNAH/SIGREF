using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using SIGREF.Common.Types;

namespace SIGREF.API.Dtos.ValueSet;

public class GetCatalogRequestDto
{

    [FromQuery]
    [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than 0")]
    public int Page { get; set; } = 1;

    [FromQuery]
    [Range(1, 500, ErrorMessage = "PageSize must be between 1 and 500")]
    public int PageSize { get; set; } = 100;
}