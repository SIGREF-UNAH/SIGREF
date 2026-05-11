using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace SIGREF.API.Utils;

// AddCommonErrorResponsesConvention.cs
public class AddCommonErrorResponsesConvention : IApplicationModelConvention
{
    public void Apply(ApplicationModel application)
    {
        foreach (var controller in application.Controllers)
        {
            foreach (var action in controller.Actions)
            {
                // Evitar controllers que no son API
                if (!controller.Attributes.OfType<ApiControllerAttribute>().Any())
                    continue;

                foreach (var errorResponse in ErrorResponsesConfiguration.CommonErrorResponses)
                {
                    var statusCode = errorResponse.Key;
                    var responseType = errorResponse.Value;

                    // Verifica si YA existe este status code en el action
                    var alreadyDefined = action.Filters
                        .OfType<ProducesResponseTypeAttribute>()
                        .Any(f => f.StatusCode == statusCode);

                    if (!alreadyDefined)
                    {
                        action.Filters.Add(
                            new ProducesResponseTypeAttribute(responseType, statusCode));
                    }
                }
            }
        }
    }
}