#nullable enable
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using SIGREF.API.Dtos.Common;

namespace SIGREF.API.Dtos
{
    public class OrganizationFilterDto : PagedFilterBase
    {
        public string? Name { get; set; }

        public bool? Active { get; set; }

        public List<OrganizationTypeEnum>? Types { get; set; }

        public string? PartOf { get; set; }
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum OrganizationTypeEnum
    {
        [EnumMember(Value = "prov")]
        Provider,

        [EnumMember(Value = "dept")]
        Department,

        [EnumMember(Value = "team")]
        Team,

        [EnumMember(Value = "govt")]
        Government,

        [EnumMember(Value = "ins")]
        Insurer,

        [EnumMember(Value = "pay")]
        Payer,

        [EnumMember(Value = "edu")]
        Educational,

        [EnumMember(Value = "reli")]
        Regligious,

        [EnumMember(Value = "crs")]
        ClinicalResearchSponsor,

        [EnumMember(Value = "cg")]
        CommunityGroup,

        [EnumMember(Value = "bus")]
        NonHealthcareBusiness,

        [EnumMember(Value = "ntwk")]
        Network
    }
}
