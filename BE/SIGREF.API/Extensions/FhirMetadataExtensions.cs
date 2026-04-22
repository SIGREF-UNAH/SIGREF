using Hl7.Fhir.Model;
using SIGREF.API.Helpers;
using SIGREF.Infrastructure.Keycloak.Interfaces;

namespace SIGREF.API.Extensions;

public static class FhirTraceabilityExtensions
{
    public static T ApplySigrefTraceability<T>(
        this T resource, 
        IUserContextService userContext, 
        IFhirNamespaceService ns) where T : Resource
    {
        if (resource == null) return null;

        // Asegurar que Meta existe
        resource.Meta ??= new Meta();

        // 1. Atribución de Autoría (sigref://occi_01/usuarios/guid)
        resource.Meta.Source = ns.GetUserSource(userContext.GetUserId());

        // 2. Correlación de Eventos (Tag con Correlation ID)
        resource.Meta.Tag ??= new List<Coding>();
        
        // Obtenemos el Correlation ID (TraceIdentifier) desde el contexto
        // Nota: Asegúrate de añadir GetCorrelationId() a tu IUserContextService
        var correlationId = userContext.GetCorrelationId() ?? "no-id"; 

        var auditTag = resource.Meta.Tag.FirstOrDefault(t => t.System == ns.AuditSystem);
        if (auditTag != null)
        {
            auditTag.Code = correlationId;
        }
        else
        {
            resource.Meta.Tag.Add(new Coding(ns.AuditSystem, correlationId));
        }

        return resource;
    }
}