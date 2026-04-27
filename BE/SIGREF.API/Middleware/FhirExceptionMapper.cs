using System.Net;
using Hl7.Fhir.Rest;
using SIGREF.API.Constants;
using SIGREF.API.Exceptions;

namespace SIGREF.API.Middleware;

public static class FhirExceptionMapper
{
    public static Exception Map(FhirOperationException ex, string resourceId, string action)
    {
        var extraData = new Dictionary<string, object>
        {
            { "ResourceId", resourceId },
            { "Action", action },
            { "FhirStatus", ex.Status.ToString() },
            { "OperationOutcome", ex.Outcome?.ToString() ?? "No detail provided" }
        };

        return ex.Status switch
        {
            HttpStatusCode.NotFound => new NotFoundException(MessageCodes.NotFound, extraData),
            HttpStatusCode.Forbidden => new ForbiddenException(MessageCodes.Forbidden, extraData),
            HttpStatusCode.Conflict => new ConflictException(MessageCodes.DbConcurrencyConflict, extraData),
            HttpStatusCode.BadRequest => new ValidationException(MessageCodes.ValidationError, extraData),
            HttpStatusCode.UnprocessableEntity => new BusinessRuleException(MessageCodes.BusinessRuleViolation, extraData),
            HttpStatusCode.Unauthorized => new SessionExpiredException(MessageCodes.Unauthorized, extraData),
            
            // Cubrimos errores de infraestructura de red/FHIR
            HttpStatusCode.BadGateway => new ExternalServiceException(MessageCodes.BadGateway, 502, extraData),
            HttpStatusCode.GatewayTimeout => new ExternalServiceException(MessageCodes.BadGateway, 504, extraData),
            HttpStatusCode.ServiceUnavailable => new ExternalServiceException(MessageCodes.BadGateway, 503, extraData),

            // Cualquier otro error de FHIR que no hayamos mapeado específicamente
            // Usamos el status que nos da FHIR en lugar de inventar uno
            _ => new ExternalServiceException(
                MessageCodes.InternalServerError, 
                (int)ex.Status, 
                extraData)
        };
    }
}