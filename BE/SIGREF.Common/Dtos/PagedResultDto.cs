using System.Text.Json.Serialization;


namespace SIGREF.Common.Dtos;

public class PaginationDto
{
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public long TotalItems { get; set; }
    public int TotalPages { get; set; }
    public bool HasPrevious { get; set; }
    public bool HasNext { get; set; }
}

public class PagedResultDto<T>
{
    public IEnumerable<T> Items { get; set; } = new List<T>();
    public PaginationDto Pagination { get; set; } = new PaginationDto();
}

