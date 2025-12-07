using Newtonsoft.Json;

namespace SIGREF.API.Dtos.Common;

public class PaginationDto
{
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    // tipo ? para Keycloak
    public int? TotalItems { get; set; }
    // tipo ? para keycloak
    public int? TotalPages { get; set; }
    public bool HasPrevious { get; set; }
    public bool HasNext { get; set; }
}

/**
 * TODO: Eliminar redundancia entre [PagedResult<T>] y [PagedResultDto<T>]
 * Ambas clases tienen la misma estructura y funcionalidad.
 * Se recomienda mantener solo una de ellas para evitar confusión y duplicidad.
 * Considerar usar ApiResponse<PaginationDto<T>> para respuestas paginadas.
 */

public class PagedResult<T>
{
    public IEnumerable<T> Items { get; set; } = new List<T>();
    public PaginationDto Pagination { get; set; } = new PaginationDto();
}

public class PagedResultDto<T>
{
    public IEnumerable<T> Items { get; set; } = new List<T>();
    public PaginationDto Pagination { get; set; } = new PaginationDto();
}


// Clasificacion sencilla de respuesta API INTERNA
public class ResponseDto<T>
{
    public T Data { get; set; }
    public string Message { get; set; } = string.Empty;

    [JsonIgnore]
    public int StatusCode { get; set; }
    public bool Status { get; set; }
}

public class PaginationDtoSigref<T>
{
    public bool HasNextPage => CurrentPage < TotalPages;
    public bool HasPreviousPage => CurrentPage > 1;
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
    public List<T> Items { get; set; } = new List<T>();
}