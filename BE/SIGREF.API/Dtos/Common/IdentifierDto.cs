namespace SIGREF.API.Dtos.Common
{
    public class IdentifierDto
    {
        public string Use { get; set; } // "official", "temp", etc.
        public string Type { get; set; } // Texto: "DNI", "Pasaporte", etc.
        public string System { get; set; } // URI del sistema emisor
        public string Value { get; set; } // Valor del ID
    }
}
