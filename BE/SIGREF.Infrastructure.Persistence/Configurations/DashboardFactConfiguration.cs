using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGREF.Core.Entity.Dashboard;

namespace SIGREF.Infrastructure.Persistence.Configurations;

public class DashboardFactConfiguration : IEntityTypeConfiguration<DashboardFact>
{
    public void Configure(EntityTypeBuilder<DashboardFact> b)
    {
        b.ToView("mv_dashboard_facts"); // si es VIEW normal, igual sirve
        b.HasNoKey();

        b.Property(x => x.InvoiceId).HasColumnName("invoice_id");
        b.Property(x => x.ItemId).HasColumnName("item_id");

        b.Property(x => x.CreatedDate).HasColumnName("created_date");

        b.Property(x => x.FinalTotal).HasColumnName("final_total");
        b.Property(x => x.RealIncome).HasColumnName("real_income");

        b.Property(x => x.InvoiceType).HasColumnName("invoice_type");
        b.Property(x => x.Status).HasColumnName("status");

        b.Property(x => x.ShiftId).HasColumnName("shift_id");
        b.Property(x => x.ShiftName).HasColumnName("shift_name");

        b.Property(x => x.LocationId).HasColumnName("location_id");

        b.Property(x => x.ServiceId).HasColumnName("service_id");
        b.Property(x => x.FhirServiceId).HasColumnName("fhir_service_id");

        b.Property(x => x.PackageId).HasColumnName("package_id");

        b.Property(x => x.PatientIdFhir).HasColumnName("patient_id_fhir");

        b.Property(x => x.TotalItems).HasColumnName("total_items");

        b.Property(x => x.CashierSessionId).HasColumnName("cashier_session_id");
    }
}