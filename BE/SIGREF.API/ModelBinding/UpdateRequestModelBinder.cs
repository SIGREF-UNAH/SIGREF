using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;
using SIGREF.Common.Dtos;

namespace SIGREF.API.ModelBinding;

public sealed class UpdateRequestModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        if (!typeof(UpdateRequestDto).IsAssignableFrom(context.Metadata.ModelType) ||
            context.Metadata.ModelType.IsAbstract)
            return null;

        return new UpdateRequestModelBinder();
    }
}

public sealed class UpdateRequestModelBinder : IModelBinder
{
    public async Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var request = bindingContext.HttpContext.Request;
        request.EnableBuffering();

        if (request.Body.CanSeek) request.Body.Position = 0;

        using var document = await JsonDocument.ParseAsync(request.Body, cancellationToken: bindingContext.HttpContext.RequestAborted);

        if (request.Body.CanSeek) request.Body.Position = 0;

        if (document.RootElement.ValueKind != JsonValueKind.Object)
        {
            bindingContext.ModelState.AddModelError(bindingContext.ModelName, "El cuerpo de una actualización debe ser un objeto JSON.");
            return;
        }

        var options = bindingContext.HttpContext.RequestServices
            .GetRequiredService<IOptions<JsonOptions>>()
            .Value
            .JsonSerializerOptions;

        var dto = JsonSerializer.Deserialize(document.RootElement.GetRawText(), bindingContext.ModelType, options) as UpdateRequestDto;
        if (dto == null)
        {
            bindingContext.ModelState.AddModelError(bindingContext.ModelName, "No fue posible deserializar la actualización.");
            return;
        }

        dto.SetSpecifiedProperties(document.RootElement.EnumerateObject().Select(property => property.Name));
        bindingContext.Result = ModelBindingResult.Success(dto);
    }
}
