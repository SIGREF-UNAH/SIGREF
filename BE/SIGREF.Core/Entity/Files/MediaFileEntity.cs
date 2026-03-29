using SIGREF.Common.Types;
using SIGREF.Core.Entity.common;

namespace SIGREF.Core.Entity.Files;
public class MediaFileEntity : BaseEntity
{
    public string FileName { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public string RelativePath { get; set; } = null!;  // ej: /media/hospital/logo.png
    public string? Description { get; set; }
    public string? SystemDescription { get; set; }
    public MediaFileType Type { get; set; }
    public long SizeBytes { get; set; }
}