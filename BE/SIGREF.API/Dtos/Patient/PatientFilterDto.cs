#nullable enable
using Hl7.Fhir.Model;
using SIGREF.API.Dtos.Common;
using System;

namespace SIGREF.API.Dtos.Patient
{
    public class PatientFilterDto : PagedFilterBase
    {
        public string? Name { get; set; }
        public AdministrativeGender? Gender { get; set; }
        public string? IdentifierType { get; set; }     
        public string? IdentifierValue { get; set; }
        // public string? Nationality { get; set; }        
        public bool? Active { get; set; }
        public DateTime? BirthDate { get; set; }        
    }
}