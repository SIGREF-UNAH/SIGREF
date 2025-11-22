using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGREF.API.Database.Entity.Administration;

namespace SIGREF.API.Database.Configurations;

    public class UserLinkConfiguration : IEntityTypeConfiguration<UserLinkEntity>
{
    public void Configure(EntityTypeBuilder<UserLinkEntity> builder)
    {
        // ============================
        //         TABLE
        // ============================
        builder.ToTable("user_links", t =>
        {
            t.HasComment("Tabla que enlaza usuarios de Keycloak con Practitioner FHIR dentro del sistema SIGREF.");
        });

        // ============================
        //       PRIMARY KEY
        // ============================
        builder.HasKey(u => u.Id);

        // ============================
        //      COLUMN MAPPINGS
        // ============================

        builder.Property(u => u.KeycloakUserId)
            .HasColumnName("keycloak_user_id")
            .HasMaxLength(64)
            .IsRequired()
            .HasComment("ID del usuario en Keycloak (JWT claim 'sub').");

        builder.Property(u => u.PractitionerId)
            .HasColumnName("practitioner_id")
            .HasMaxLength(64)
            .IsRequired()
            .HasComment("ID del Practitioner en HAPI FHIR asociado al usuario.");

        builder.Property(u => u.LastSync)
            .HasColumnName("last_sync")
            .HasComment("Fecha de última sincronización de datos.");

        builder.Property(u => u.Username)
            .HasColumnName("username")
            .HasMaxLength(256)
            .HasComment("Username en Keycloak (reflejo).");

        builder.Property(u => u.Email)
            .HasColumnName("email")
            .HasMaxLength(256)
            .HasComment("Correo electrónico del usuario (reflejo de Keycloak).");

        builder.Property(u => u.DisplayName)
            .HasColumnName("display_name")
            .HasMaxLength(256)
            .HasComment("Nombre mostrado del Practitioner.");

        builder.Property(u => u.SyncStatus)
            .HasColumnName("sync_status")
            .HasMaxLength(32)
            .HasComment("Estado de sincronización del usuario.");

        builder.Property(u => u.Active)
            .HasColumnName("is_active")
            .IsRequired()
            .HasComment("Estado activo/inactivo del usuario.");

        builder.Property(u => u.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired()
            .HasComment("Fecha de creación del registro.");

        builder.Property(u => u.UpdatedAt)
            .HasColumnName("updated_at")
            .HasComment("Fecha de actualización del registro.");

        builder.Property(u => u.CreatedById)
            .HasColumnName("created_by_id")
            .IsRequired()
            .HasComment("Usuario que creó este registro (incluye SYSTEM).");

        builder.Property(u => u.UpdatedById)
            .HasColumnName("updated_by_id")
            .HasComment("Último usuario que actualizó este registro.");

        // ============================
        //        RELACIONES
        // ============================

        builder.HasOne(u => u.CreatedBy)
            .WithMany()
            .HasForeignKey(u => u.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(u => u.UpdatedBy)
            .WithMany()
            .HasForeignKey(u => u.UpdatedById)
            .OnDelete(DeleteBehavior.Restrict);

        // ============================
        //         ÍNDICES
        // ============================

        // UNIQUE — no pueden existir 2 usuarios con el mismo KeycloakID
        builder.HasIndex(u => u.KeycloakUserId)
            .IsUnique()
            .HasDatabaseName("idx_userlink_keycloak_unique");

        // UNIQUE — cada practitioner tiene un solo UserLink
        builder.HasIndex(u => u.PractitionerId)
            .IsUnique()
            .HasDatabaseName("idx_userlink_practitioner_unique");

        builder.HasIndex(u => u.Email)
            .HasDatabaseName("idx_userlink_email");

        builder.HasIndex(u => u.Username)
            .HasDatabaseName("idx_userlink_username");

        builder.HasIndex(u => u.DisplayName)
            .HasDatabaseName("idx_userlink_displayname");

        builder.HasIndex(u => u.Active)
            .HasDatabaseName("idx_userlink_is_active");

        builder.HasIndex(u => u.SyncStatus)
            .HasDatabaseName("idx_userlink_sync_status");

        builder.HasIndex(u => u.LastSync)
            .HasDatabaseName("idx_userlink_last_sync");

        builder.HasIndex(u => u.CreatedById)
            .HasDatabaseName("idx_userlink_created_by");

        builder.HasIndex(u => u.UpdatedById)
            .HasDatabaseName("idx_userlink_updated_by");
    }
}