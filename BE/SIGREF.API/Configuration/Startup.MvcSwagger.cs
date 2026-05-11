using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.OpenApi;
using SIGREF.API.Utils;
using Swashbuckle.AspNetCore.Annotations;

namespace SIGREF.API;

public partial class Startup
{
    private void AddControllersAndSwagger(IServiceCollection services)
    {
        services.AddControllers(options =>
        {
            options.Conventions.Add(new AddCommonErrorResponsesConvention());
        });
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            // Información básica del documento OpenAPI
            c.SwaggerDoc("v1", new OpenApiInfo 
            { 
                Title = "SIGREF API", 
                Version = "1.0.0",
                Description = "API de Auditoría para Sistema de Gestión de Referencias FHIR",
                Contact = new OpenApiContact 
                { 
                    Name = "Equipo SIGREF", 
                    Email = "soporte@sigref.hn" 
                }
            });

            //  Definir tags con descripción (esto crea las "burbujas" en Swagger UI)
            c.TagActionsBy(api =>
            {
                //  Intentar obtener GroupName definido en [ApiExplorerSettings]
                if (!string.IsNullOrEmpty(api.GroupName))
                {
                    return new[] { api.GroupName };
                }
    
                //  Fallback: usar nombre del controller (sin "Controller")
                var controller = api.ActionDescriptor.RouteValues["controller"]?.ToString();
                if (!string.IsNullOrEmpty(controller))
                {
                    return new string[] { controller }; // Tipo explícito para evitar error de inferencia
                }
    
                //  Último recurso
                return new[] { "General" };
            });

            // Ordenar endpoints dentro de cada tag: controller + path
            c.OrderActionsBy(apiDesc => 
            {
                var controller = apiDesc.ActionDescriptor.RouteValues["controller"]?.ToString() ?? "ZZZ";
                var path = apiDesc.RelativePath ?? "";
                var method = apiDesc.HttpMethod ?? "GET";
                return $"{controller}_{method}_{path}";
            });
            // Incluir comentarios XML de documentación (opcional pero recomendado)
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                c.IncludeXmlComments(xmlPath);
            }

            // Habilitar atributos de Swashbuckle.Annotations ([SwaggerOperation], [SwaggerResponse], etc.)
            c.EnableAnnotations();

            // Tus filtros personalizados
            c.SchemaFilter<EnumSchemaFilter>();
        });
    }
}
