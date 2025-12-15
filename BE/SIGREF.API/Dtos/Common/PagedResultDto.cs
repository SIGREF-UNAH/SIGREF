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
