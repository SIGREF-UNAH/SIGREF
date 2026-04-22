using Hl7.Fhir.Model;
using SIGREF.API.Helpers;
using SIGREF.Infrastructure.Keycloak.Interfaces;

namespace SIGREF.API.Fhir;

/// <summary>
/// Clase base para todos los servicios que interactúan con el servidor FHIR.
/// Centraliza la lógica de trazabilidad y auditoría aplicada a los recursos.
/// </summary>
public abstract class BaseFhirService
{
    private readonly IUserContextService _userContext;
 
    /// <summary>
    /// Servicio de namespaces FHIR expuesto a las clases derivadas para
    /// construcción de identificadores, extensiones y URIs de perfil.
    /// </summary>
    protected readonly IFhirNamespaceService _ns;
 
    protected BaseFhirService(
        IUserContextService userContext,
        IFhirNamespaceService ns)
    {
        _userContext = userContext;
        _ns          = ns;
    }
 
    /// <summary>
    /// Aplica metadatos de trazabilidad SIGREF a cualquier recurso FHIR
    /// antes de enviarlo al servidor. Debe llamarse en Create y Update.
    /// </summary>
    /// <typeparam name="T">Tipo de recurso FHIR.</typeparam>
    /// <param name="resource">Recurso al que se le aplicarán los metadatos.</param>
    /// <param name="isCreate">
    /// <c>true</c> al crear (establece <c>VersionId = "1"</c>);
    /// <c>false</c> al actualizar.
    /// </param>
    protected void ApplyMeta<T>(T resource, bool isCreate = true) where T : Resource
    {
        resource.Meta ??= new Meta();
        resource.Meta.LastUpdated = DateTimeOffset.UtcNow;
 
        if (isCreate)
            resource.Meta.VersionId = "1";
 
        // Atribución de autoría: sigref://occi_01/usuarios/{guid}
        resource.Meta.Source = _ns.GetUserSource(_userContext.GetUserId());
 
        // Correlación del request HTTP
        resource.Meta.Tag ??= new List<Coding>();
        var correlationId = _userContext.GetCorrelationId() ?? "no-id";
 
        var auditTag = resource.Meta.Tag
            .FirstOrDefault(t => t.System == _ns.AuditSystem);
 
        if (auditTag is not null)
            auditTag.Code = correlationId;
        else
            resource.Meta.Tag.Add(new Coding(_ns.AuditSystem, correlationId));
    }
}
 