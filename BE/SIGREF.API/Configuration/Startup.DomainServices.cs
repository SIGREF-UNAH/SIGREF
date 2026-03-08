using SIGREF.API.Services.AdministrationHospital;
using SIGREF.API.Services.Billing;
using SIGREF.API.Services.Bridges;
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
using SIGREF.Common.Interfaces;
using SIGREF.Infrastructure.Keycloak;
using SIGREF.Infrastructure.Reporting;

namespace SIGREF.API;

public partial class Startup
{
    private void AddDomainServices(IServiceCollection services, WebApplicationBuilder applicationBuilder)
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
        // NUEVO: Registrar el puente para que Keycloak pueda validar médicos
        services.AddScoped<IFhirPractitionerService, FhirPractitionerBridge>();

        // ================= Keycloak (LIBRERIA) =================
        // Este método ya registra el Client, el AdminService y el UserContext
        services.AddKeycloakInfrastructure(applicationBuilder.Configuration);

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

        
        //   LIBRERIAS INTERNAS
        // ================= PDFs =====================
        applicationBuilder.AddReportingInfrastructure();
    }
}
