using SIGREF.API.Utils;

namespace SIGREF.API;

public partial class Startup
{
    private void AddControllersAndOpenApi(IServiceCollection services)
    {
        services.AddControllers(options =>
        {
            options.Conventions.Add(new AddCommonErrorResponsesConvention());
        });

        services.AddEndpointsApiExplorer();
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, _, _) =>
            {
                document.Info.Title = "SIGREF API";
                document.Info.Version = "1.0.0";
                document.Info.Description = "API de Auditoría para Sistema de Gestión de Referencias FHIR";
                document.Info.Contact = new Microsoft.OpenApi.Models.OpenApiContact
                {
                    Name = "Equipo SIGREF",
                    Email = "soporte@sigref.hn"
                };

                return Task.CompletedTask;
            });
        });
    }
}
