using SIGREF.API.Constants;
using SIGREF.API.Database.Seeding;
using SIGREF.API.Services.Common;
using Microsoft.Extensions.Logging;
using Hl7.Fhir.Model;
using System.Threading;
using System.Threading.Tasks;

namespace SIGREF.API.Controllers.Seeder
{
    public class PractitionerRolesSeeder : TerminologySeederBase
    {
        public PractitionerRolesSeeder(FhirService fhirService, ILogger<PractitionerRolesSeeder> logger)
            : base(fhirService, logger) { }

        public System.Threading.Tasks.Task SeedAsync(CancellationToken cancellationToken = default)
        {
            var def = new TerminologyDefinition
            {
                CodeSystemUrl = "http://terminology.hl7.org/CodeSystem/practitioner-role",
                CodeSystemName = "PractitionerRoleTypes_HL7",
                CodeSystemTitle = "FHIR Practitioner Roles",
                Concepts = new Dictionary<string, string>
                {
                    ["doctor"] = "Doctor",
                    ["nurse"] = "Nurse",
                    ["pharmacist"] = "Pharmacist",
                    ["receptionist"] = "Receptionist",
                    ["laboratory-technician"] = "Laboratory Technician",
                    ["physiotherapist"] = "Physiotherapist",
                    ["dietitian"] = "Dietitian",
                    ["optometrist"] = "Optometrist",
                    ["dentist"] = "Dentist",
                    ["psychologist"] = "Psychologist",
                    ["midwife"] = "Midwife",
                    ["paramedic"] = "Paramedic",
                    ["anesthetist"] = "Anesthetist",
                    ["cardiologist"] = "Cardiologist",
                    ["radiologist"] = "Radiologist"
                },
                ValueSetUrl = "http://hl7.org/fhir/ValueSet/practitioner-role",
                ValueSetName = "ValueSetPractitionerRolesHL7",
                ValueSetTitle = "FHIR Practitioner Roles"
            };

            return SeedTerminologyAsync(def, cancellationToken);
        }
    }
}

