using Hl7.Fhir.Rest;
using Microsoft.Extensions.Options;
using SIGREF.API.Constants;

namespace SIGREF.API.Services;

/// <summary>
/// Servicio para interactuar con un servidor FHIR (Fast Healthcare Interoperability Resources).
/// Proporciona un cliente configurado para realizar operaciones sobre recursos FHIR.
/// </summary>
public class FhirService
{
    private readonly FhirClient _fhirClient;

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="FhirService"/>.
    /// Configura el cliente FHIR utilizando los parámetros definidos en el entorno.
    /// </summary>
    /// <param name="env">
    /// Opciones de entorno que contienen la configuración del endpoint FHIR.
    /// El objeto <see cref="Env"/> debe proporcionar la propiedad <c>Phir</c>,
    /// que incluye el <c>BaseUrl</c> del servidor FHIR.
    /// </param>
    public FhirService(IOptions<Env> env)
    {
        var fhirEndpoint = env.Value;
        _fhirClient = new FhirClient(fhirEndpoint.Fhir.BaseUrl, new FhirClientSettings()

        {
            Timeout = 10000,
            PreferredFormat = ResourceFormat.Json,
            VerifyFhirVersion = true,
            ReturnPreference = ReturnPreference.Representation,
        }
        );
    }

    /// <summary>
    /// Obtiene la instancia configurada de <see cref="FhirClient"/> para interactuar con el servidor FHIR.
    /// </summary>
    /// <returns>
    /// Instancia de <see cref="FhirClient"/> utilizada para operaciones FHIR.
    /// </returns>
    public FhirClient GetFhirClient()
    {
        return _fhirClient;
    }
}