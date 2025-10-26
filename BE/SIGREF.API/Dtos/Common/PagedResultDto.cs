namespace SIGREF.API.Dtos.Common;

public class PaginationDto
{
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
    public bool HasPrevious { get; set; }
    public bool HasNext { get; set; }
}

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