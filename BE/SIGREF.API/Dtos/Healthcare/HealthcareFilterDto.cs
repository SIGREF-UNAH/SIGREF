#nullable enable
using SIGREF.Common.Dtos;

namespace SIGREF.API.Dtos.Healthcare;

public class HealthcareFilterDto : PagedFilterBase
{
    // =========================
    //   FILTROS DE SELECCIÓN
    // =========================

    /// <summary>
    ///     Filtrar por nombre del servicio.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    ///     Filtrar por estado activo.
    ///     - true  => solo activos
    ///     - false => solo inactivos
    ///     - null  => todos
    /// </summary>
    public bool? Active { get; set; }

    /// <summary>
    ///     Filtrar por especialidad.
    /// </summary>
    public string? Specialty { get; set; }

    /// <summary>
    ///     Filtrar por organización proveedora.
    /// </summary>
    public string? ProvidedBy { get; set; }

    /// <summary>
    ///     Filtrar por ubicación.
    /// </summary>
    public string? Location { get; set; }

    public string? Abbreviation { get; set; }

    /// <summary>
    ///     Filtro por alcance del servicio.
    ///     - null      => todos
    ///     - Internal  => solo servicios internos
    ///     - External  => solo servicios visibles
    /// </summary>
    //[JsonConverter(typeof(HealthcareScope))]
    public HealthcareScope? Scope { get; set; }

    // =========================
    //   OPCIONES DE PROYECCIÓN
    // =========================

    /// <summary>
    ///     Indica si se debe incluir el costo en el resultado.
    /// </summary>
    public bool IncludeCost { get; set; } = false;
}