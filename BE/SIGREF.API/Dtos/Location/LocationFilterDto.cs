#nullable enable
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace SIGREF.API.Dtos.Location
{
    public class LocationFilterDto
    {
        public string? Name { get; set; }
        /// <summary>
        /// Estado de la ubicación (active, suspended, inactive).
        /// </summary>
        public LocationFilterStatus? Status { get; set; }

        // Paginación
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum LocationFilterStatus
    {
        [EnumMember(Value = "active")]
        Active,

        [EnumMember(Value = "suspended")]
        Suspended,

        [EnumMember(Value = "inactive")]
        Inactive
    }
}
