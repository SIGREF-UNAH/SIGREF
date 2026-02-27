using SIGREF.API.Services.AdministrationHospital;
using SIGREF.API.Services.Auth;
using SIGREF.API.Services.Billing;
using SIGREF.API.Services.Cashier;
using SIGREF.API.Services.Files;
using SIGREF.API.Services.Healthcare;
using SIGREF.API.Services.Location;
using SIGREF.API.Services.Organization;
using SIGREF.API.Services.Organizations;
using SIGREF.API.Services.Patient;
using SIGREF.API.Services.Practitioner;
using SIGREF.API.Services.PractitionerRole;
using SIGREF.API.Services.Reports;
using SIGREF.API.Services.Serie;
using SIGREF.API.Services.ServiceGroup;
using SIGREF.API.Services.ValueSet;

namespace SIGREF.API;

public partial class Startup
{
    private void AddDomainServices(IServiceCollection services)
    {
        // ================= HEALTH =================
        services.AddScoped<LocationService>();
        services.AddScoped<HealthcareFHIRService>();
        services.AddScoped<IPatientService, PatientService>();
        services.AddScoped<IPractitionerRoleService, PractitionerRoleService>();
        services.AddScoped<IPractitionerService, PractitionerService>();
        services.AddScoped<IOrganizationService, OrganizationService>();
        services.AddScoped<ServiceGroupService>();
        services.AddScoped<IValueSetService, ValueSetService>();

        // Contexto de usuario 
        services.AddScoped<IUserContextService, UserContextService>();

        // ================= SIGREF ==================
        services.AddScoped<IShiftService, ShiftService>();
        services.AddScoped<ICashierSessionService, CashierSessionService>();
        services.AddScoped<IHospitalPropertiesService, HospitalPropertiesService>();
        services.AddScoped<IMediaFileService, MediaFileService>();
        services.AddScoped<ISerieService, SerieService>();
        services.AddScoped<IInvoiceService, InvoiceService>();
        services.AddScoped<IHealthcareService, HealthcareApplicationService>();

        // ================= Reportes =================
        services.AddScoped<IReportQueryService, ReportQueryService>();
        services.AddScoped<IReportExportService, ReportExportService>();

        // ================= PDFs =====================
        services.AddScoped<ITestPdfService, TestPdfService>();
    }
}
