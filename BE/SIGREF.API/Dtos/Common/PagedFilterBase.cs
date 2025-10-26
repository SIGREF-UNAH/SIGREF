namespace SIGREF.API.Dtos.Common
{
    public abstract class PagedFilterBase
    {
        public int PageNumber { get; set; }

        public int PageSize { get; set; }
    }
}
