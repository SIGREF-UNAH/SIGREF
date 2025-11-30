using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using SIGREF.API.Database.Entity.common;
using SIGREF.API.Dtos.Common;

namespace SIGREF.API.Dtos.Files;

public class UploadMediaFileDto
{
    [Required]
    public IFormFile File { get; set; } = null!;

    [Required]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public MediaFileType Type { get; set; }

    public string? Description { get; set; }
}


public class MediaFileDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public string RelativePath { get; set; } = null!;
    public long SizeBytes { get; set; }
    public string? Description { get; set; }
    public string? SystemDescription { get; set; }
    public MediaFileType Type { get; set; }
    public string Url => $"/files/{Id}";
}

public class DeleteMediaFileDto
{
    [Required]
    public Guid Id { get; set; }
}

public class MediaFileFilterDto: PagedFilterBase
{
    public MediaFileType? Type { get; set; }
    public string? Search { get; set; }
}


