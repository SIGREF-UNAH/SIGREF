namespace SIGREF.Common.Dtos;

public abstract class PagedFilterBase
{
    public int PageNumber { get; set; }

    public int PageSize { get; set; }
}