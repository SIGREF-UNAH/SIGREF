using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGREF.API.Database.Entity.Files;

namespace SIGREF.API.Database.Configurations;

public class MediaFileConfiguration : IEntityTypeConfiguration<MediaFileEntity>
{
    public void Configure(EntityTypeBuilder<MediaFileEntity> builder)
    {
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
        // Primary Key
        // ============================

        builder.HasKey(x => x.Id);

        // BaseEntity 


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
            .HasColumnName("media_type")
            .HasComment("Tipo lógico del archivo (AppHospital, HealthGuilt).");

        builder.Property(x => x.SizeBytes)
            .HasColumnName("size_bytes")
            .HasComment("Tamaño del archivo en bytes.");
        // ============================
        //          AUDITORÍA
        // ============================
        builder.Property(x => x.CreatedById)
            .HasColumnName("created_by_id")
            .IsRequired()
            .HasComment("ID del usuario que creó el registro.");

        builder.Property(x => x.UpdatedById)
            .HasColumnName("updated_by_id")
            .HasComment("ID del usuario que realizó la última actualización.");

        builder.Property(x => x.CreatedDate)
            .HasColumnName("created_date")
            .IsRequired()
            .HasComment("Fecha de creación del turno (UTC).");

        builder.Property(x => x.UpdatedDate)
            .HasColumnName("updated_date")
            .HasComment("Fecha de última actualización (UTC).");

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