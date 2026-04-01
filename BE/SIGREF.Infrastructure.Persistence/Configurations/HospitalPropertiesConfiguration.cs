using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGREF.Core.Entity.Administration;

namespace SIGREF.Infrastructure.Persistence.Configurations;

public class HospitalPropertiesConfiguration : BaseEntityConfiguration<HospitalPropertiesEntity>
{
    public override void Configure(EntityTypeBuilder<HospitalPropertiesEntity> builder)
    {
        base.Configure(builder);
        // ===============================
        //           TABLE
        // ===============================
        builder.ToTable("hospital_properties", t =>
        {
            t.HasComment("Propiedades generales del hospital: datos administrativos, logos, contacto y configuración base. Tabla singleton.");
        });
        
        // ===============================
        //           PROPIEDADES
        // ===============================

        builder.Property(e => e.Name)
            .IsRequired()
            .HasColumnName("name")
            .HasMaxLength(200)
            .HasComment("Nombre oficial del hospital");

        builder.Property(e => e.Director)
            .HasMaxLength(150)
            .HasColumnName("director");

        builder.Property(e => e.Subdirector)
            .HasMaxLength(150)
            .HasColumnName("subdirector");

        builder.Property(e => e.Location) 
            .HasColumnName("location")
            .HasMaxLength(300);

        builder.Property(e => e.UrlLogo)
            .HasMaxLength(300)
            .HasColumnName("url_logo");

        builder.Property(e => e.UrlLogoHealth)
            .HasMaxLength(300)
            .HasColumnName("url_logo_health");

        builder.Property(e => e.PhoneNumber)
            .HasMaxLength(20)
            .HasColumnName("phone_number");

        builder.Property(e => e.Email)
            .HasMaxLength(200)
            .HasColumnName("email");

        builder.Property(e => e.HospitalCode)
            .HasMaxLength(20)
            .HasColumnName("hospital_code");

        builder.Property(e => e.RTN)
            .HasMaxLength(50)
            .HasColumnName("rtn");

        builder.Property(e => e.Website)
            .HasMaxLength(200)
            .HasColumnName("website");

        builder.Property(e => e.Currency)
            .HasMaxLength(10)
            .HasColumnName("currency");
        
        builder.Property(e => e.LogoMediaId)
            .HasColumnName("logo_media_id");
        builder.Property(e => e.HealthLogoMediaId)
            .HasColumnName("health_logo_media_id");

        //builder.Property(e => e.ExchangeVersion)
        //    .HasMaxLength(10)
        //    .HasColumnName("exchange_version");

        builder.Property(e => e.IsSingleton)
            .HasColumnName("is_singleton");
        // ===============================
        //         ÍNDICE ÚNICO REAL
        // ===============================
        builder.HasIndex(e => e.IsSingleton)
            .IsUnique()
            .HasFilter("is_singleton = true")
            .HasDatabaseName("idx_singleton_enforcer");
    }
}