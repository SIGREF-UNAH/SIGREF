using System.ComponentModel.DataAnnotations;
using SIGREF.API.Dtos.Common;

namespace SIGREF.API.Dtos.Series;

public class SerieDto
{
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Prefix { get; set; }

        // ===============================
        //       RANGO DE NUMERACIÓN
        // ===============================
        public long StartNumber { get; set; }
        public long EndNumber { get; set; }
        public long CurrentNumber { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
}

public class CreateSeriesDto
{
        [Required]
        public string Name { get; set; }
        
        [Required]
        public string Prefix { get; set; }

        // ===============================
        //       RANGO DE NUMERACIÓN
        // ===============================
        [Required]
        public long StartNumber { get; set; }
        [Required]
        public long EndNumber { get; set; }
}

public class UpdateSeriesDto : CreateSeriesDto
{
        public bool? IsActive { get; set; }
}


public class FilterSerieDto : PagedFilterBase
{
        public string? Name { get; set; }
        public string? Prefix { get; set; }
        public long? StartNumber { get; set; }
        public long? EndNumber { get; set; }
       // public long? CurrentNumber { get; set; }
        public bool? IsActive { get; set; }
        
}