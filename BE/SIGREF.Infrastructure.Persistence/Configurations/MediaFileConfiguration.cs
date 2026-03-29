using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGREF.Core.Entity.Files;

namespace SIGREF.Infrastructure.Persistence.Configurations;

public class MediaFileConfiguration : BaseEntityConfiguration<MediaFileEntity>
{
    public override void Configure(EntityTypeBuilder<MediaFileEntity> builder)
    {
        base.Configure(builder);
        // ============================
        // Información general
        // ============================

        builder.ToTable("media_files" ,
            t =>
            {
                t.HasComment(
                    "Archivos multimedia almacenados en el sistema (logos, imágenes varias).");
            });
        
        // ============================
        // Campos principales
        // ============================

        builder.Property(x => x.FileName)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("file_name")
            .HasComment("Nombre original del archivo subido.");

        builder.Property(x => x.ContentType)
            .IsRequired()
            .HasMaxLength(20)
            .HasColumnName("content_type")
            .HasComment("Tipo MIME real del archivo (image/png, image/jpeg).");

        builder.Property(x => x.RelativePath)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnName("relative_path")
            .HasComment("Ruta relativa pública del archivo: /media/...");

        builder.Property(x => x.Description)
            .HasMaxLength(255)
            .HasColumnName("description")
            .HasComment("Descripción asignada por el usuario.");

        builder.Property(x => x.SystemDescription)
            .HasMaxLength(255)
            .HasColumnName("system_description")
            .HasComment("Descripción del archivo generada por el sistema.");

        builder.Property(x => x.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasColumnName("media_type")
            .HasComment("Tipo lógico del archivo (AppHospital, HealthGuilt).");

        builder.Property(x => x.SizeBytes)
            .HasColumnName("size_bytes")
            .HasComment("Tamaño del archivo en bytes.");

        // ============================
        // Índices
        // ============================

        builder.HasIndex(x => x.Type)
            .HasDatabaseName("idx_media_files_type");


        builder.HasIndex(x => x.FileName)
            .HasDatabaseName("idx_media_files_filename");


        builder.HasIndex(x => x.CreatedDate)
            .HasDatabaseName("idx_media_files_created_date");

    }
}