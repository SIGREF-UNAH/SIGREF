using Hl7.Fhir.Model;
using SIGREF.Common.Dtos;

namespace SIGREF.API.Helpers
{
    /// <summary>
    /// Helper genérico para manejar la paginación de recursos FHIR.
    /// </summary>
    public static class FhirPaginationHelper
    {
        /// <summary>
        /// Normaliza los valores de número de página y tamaño y calcula el offset.
        /// </summary>
        public static (int PageNumber, int PageSize, int Offset) Normalize(int pageNumber, int pageSize)
        {
            var validPageNumber = Math.Max(1, pageNumber);
            var validPageSize = pageSize > 0 ? pageSize : 10;
            var offset = (validPageNumber - 1) * validPageSize;

            return (validPageNumber, validPageSize, offset);
        }

        /// <summary>
        /// Construye un objeto <see cref="PaginationDto"/> a partir del total de elementos y la página actual.
        /// </summary>
        public static PaginationDto BuildPagination(int totalItems, int pageNumber, int pageSize, bool hasNextLink)
        {
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            return new PaginationDto
            {
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = totalPages,
                HasPrevious = pageNumber > 1,
                HasNext = hasNextLink || pageNumber < totalPages
            };
        }

        /// <summary>
        /// Extrae los recursos de tipo <typeparamref name="T"/> de un <see cref="Bundle"/> y devuelve un PagedResult.
        /// </summary>
        public static PagedResultDto<T> ToPagedResult<T>(Bundle bundle, int pageNumber, int pageSize) where T : Resource
        {
            var items = bundle.Entry?
                .Where(e => e.Resource is T)
                .Select(e => (T)e.Resource)
                .ToList() ?? new List<T>();

            var totalItems = bundle.Total ?? items.Count;

            var pagination = BuildPagination(
                totalItems,
                pageNumber,
                pageSize,
                hasNextLink: bundle.NextLink != null
            );

            return new PagedResultDto<T>
            {
                Items = items,
                Pagination = pagination
            };
        }
        
        /// <summary>
        /// Extrae los recursos de tipo <typeparamref name="T"/> de un <see cref="Bundle"/> y devuelve un PagedResult.
        /// </summary>
        public static (Bundle,PaginationDto ) ToPagedResult(Bundle bundle, int pageNumber, int pageSize)
        {
 
            var totalItems = bundle.Total?? 0;

            PaginationDto pagination = BuildPagination(
                totalItems,
                pageNumber,
                pageSize,
                hasNextLink: bundle.NextLink != null
            );

            return (bundle, pagination);
        }
    }
}
