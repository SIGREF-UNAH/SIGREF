using SIGREF.Core.Entity.common;

namespace SIGREF.Core.Entity.Catalogs;

public class HealthService : BaseEntity
{
    public string HealthServiceFhirId { get; set; }

    // ======================
    //   PRECIO
    // ======================
    public decimal Price { get; set; }
}