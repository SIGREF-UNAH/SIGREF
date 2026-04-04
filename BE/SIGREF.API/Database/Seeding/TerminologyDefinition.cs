namespace SIGREF.API.Database.Seeding;

/// <summary>
/// Representa la definición declarativa de un catálogo de terminología FHIR.
/// Agrupa los metadatos del CodeSystem y su ValueSet asociado.
/// </summary>
public class TerminologyDefinition
{
    /// <summary>URL canónica única del CodeSystem.</summary>
    public string CodeSystemUrl { get; set; } = default!;
 
    /// <summary>Nombre técnico del CodeSystem (formato canónico).</summary>
    public string CodeSystemName { get; set; } = default!;
 
    /// <summary>Título legible del CodeSystem.</summary>
    public string CodeSystemTitle { get; set; } = default!;
 
    /// <summary>
    /// Conceptos del catálogo. Clave = código, Valor = descripción (display).
    /// </summary>
    public Dictionary<string, string> Concepts { get; set; } = new();
 
    /// <summary>URL canónica única del ValueSet.</summary>
    public string ValueSetUrl { get; set; } = default!;
 
    /// <summary>Nombre técnico del ValueSet.</summary>
    public string ValueSetName { get; set; } = default!;
 
    /// <summary>Título legible del ValueSet.</summary>
    public string ValueSetTitle { get; set; } = default!;
}