 using Hl7.Fhir.Model;
using SIGREF.API.Dtos.Common;
using System.ComponentModel.DataAnnotations;

namespace SIGREF.API.Dtos;

public class ExtendedContactDetailDto
{
    public string? Name { get; set; }
    public List<ContactPointDto>? Telecom { get; set; }
    public AddressDto? Address { get; set; }
}
public class CodingDto
{
    public string? System { get; set; }
    public string? Code { get; set; }
    public string? Display { get; set; }
}
public class ReferenceDto
{
    public string? Reference { get; set; }
    public string? Type { get; set; }
    public string? Display { get; set; }
}
public class CodeableConceptDto
{
    public List<CodingDto> Coding { get; set; } = new();
    public string? Text { get; set; }
}
