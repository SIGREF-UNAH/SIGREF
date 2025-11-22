using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGREF.API.Database.Entity.Administration;

namespace SIGREF.API.Database.Configurations;

public class HospitalPropertiesConfiguration : IEntityTypeConfiguration<HospitalPropertiesEntity>
{
    public void Configure(EntityTypeBuilder<HospitalPropertiesEntity> builder)
    {
        // ===============================
        //           TABLE
        // ===============================
        builder.ToTable("hospital_properties", t =>
        {
            t.HasComment("Propiedades generales del hospital: datos administrativos, logos, contacto y configuración base. Tabla singleton.");
        });

        // ===============================
        //           PRIMARY KEY
        // ===============================
        builder.HasKey(e => e.Id);

        // ===============================
        //           PROPIEDADES
        // ===============================

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200)
            .HasColumnName("name");

        builder.Property(e => e.Director)
            .HasMaxLength(150)
            .HasColumnName("director");

        builder.Property(e => e.Subdirector)
            .HasMaxLength(150)
            .HasColumnName("subdirector");

        builder.Property(e => e.Ubication)
            .HasMaxLength(300)
            .HasColumnName("location");

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

        builder.Property(e => e.ExchangeVersion)
            .HasMaxLength(10)
            .HasColumnName("exchange_version");

        builder.Property(e => e.IsSingleton)
            .HasColumnName("is_singleton");

        // ===============================
        //         ÍNDICE ÚNICO REAL
        // ===============================
        builder.HasIndex(e => e.IsSingleton)
            .IsUnique()
            .HasDatabaseName("idx_singleton_enforcer");
    }
}