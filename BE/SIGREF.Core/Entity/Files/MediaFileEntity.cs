using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SIGREF.Common.Types;
using SIGREF.Core.Entity.common;

namespace SIGREF.Core.Entity.Files;
[Table("media_files")]
public class MediaFileEntity : BaseEntity
{
    [Required]
    [Column("file_name")]
    [StringLength(100)]
    public string FileName { get; set; } = null!;

    [Required]
    [Column("content_type")]
    [StringLength(20)]
    public string ContentType { get; set; } = null!;

    [Required]
    [Column("relative_path")]
    [StringLength(255)]
    public string RelativePath { get; set; } = null!;  // ej: /media/hospital/logo.png

    [Column("description")]
    [StringLength(255)]
    public string? Description { get; set; }
    
    [Column("system_description")]
    [StringLength(255)]
    public string? SystemDescription { get; set; }
    
    [Required]
    [Column("media_type")]
    public MediaFileType Type { get; set; }


    [Column("size_bytes")]
    public long SizeBytes { get; set; }
}