using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGREF.Core.Entity.common;

namespace SIGREF.Infrastructure.Persistence.Configurations;

public abstract class BaseEntityConfiguration<TBase> : IEntityTypeConfiguration<TBase> 
    where TBase : BaseEntity
{
    public virtual void Configure(EntityTypeBuilder<TBase> builder)
    {
        // PK
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .IsRequired();

        // ESTADO
        builder.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true)
            .IsRequired();

        // AUDITORÍA (IDs de usuario)
        builder.Property(x => x.CreatedById)
            .HasColumnName("created_by_id")
            .IsRequired();

        builder.Property(x => x.UpdatedById)
            .HasColumnName("updated_by_id");

        // AUDITORIA 
        builder.Property(x => x.CreatedDate)
            .HasColumnName("created_date")
            .IsRequired(); // EF Core pondrá timestamptz en PSQL o datetimeoffset en SQL Server

        builder.Property(x => x.UpdatedDate)
            .HasColumnName("updated_date");
        
    }
}