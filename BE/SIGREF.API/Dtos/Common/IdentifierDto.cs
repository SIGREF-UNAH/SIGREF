using System.Text.Json.Serialization;
using Hl7.Fhir.Model;

namespace SIGREF.API.Dtos.Common;

    public class IdentifierDto
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Identifier.IdentifierUse? Use { get; set; }  
        public string Type { get; set; } // Texto: "DNI", "Pasaporte", etc.
        public string System { get; set; } // URI del sistema emisor
        public string Value { get; set; } // Valor del ID
    }

