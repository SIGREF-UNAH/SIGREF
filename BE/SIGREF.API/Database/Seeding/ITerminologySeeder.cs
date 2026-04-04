namespace SIGREF.API.Database.Seeding;

/// <summary>
/// Contrato que deben implementar todos los inicializadores de catálogos base de terminología FHIR.
/// </summary>
/// <remarks>
/// Cada implementación representa un catálogo hospitalario (CodeSystem + ValueSet).
/// El orquestador <see cref="SIGREFSeeder"/> los descubre y ejecuta automáticamente
/// al arrancar el sistema, sin necesidad de modificar el orquestador.
///
/// Flujo por catálogo al arrancar:
///   - Si no existe  = se crea (SeedAsync).
///   - Si ya existe  = se omite (comportamiento por defecto).
///   - Si ya existe y el seeder implementa <see cref="IUpdatableTerminologySeeder"/>
///     = se sincronizan conceptos nuevos sin tocar los existentes (UpdateAsync).
/// </remarks>
public interface ITerminologySeeder
{
    /// <summary>Nombre descriptivo del catálogo. Usado en logging.</summary>
    string CatalogName { get; }
 
    /// <summary>
    /// Crea el CodeSystem y ValueSet si no existen aún en el servidor FHIR.
    /// </summary>
    Task SeedAsync(CancellationToken cancellationToken = default);
}
 
/// <summary>
/// Extensión opcional de <see cref="ITerminologySeeder"/> para catálogos
/// que pueden recibir conceptos adicionales con el tiempo.
/// </summary>
/// <remarks>
/// Implementa esta interfaz cuando un catálogo puede crecer (ej. nuevas especialidades,
/// nuevos tipos de ubicación). El orquestador la detecta automáticamente y llama
/// a <see cref="UpdateAsync"/> después de <see cref="ITerminologySeeder.SeedAsync"/>.
///
/// Garantía: nunca elimina conceptos existentes, solo agrega los que faltan.
/// </remarks>
public interface IUpdatableTerminologySeeder : ITerminologySeeder
{
    /// <summary>
    /// Agrega al CodeSystem y ValueSet existentes los conceptos que aún no estén presentes.
    /// No modifica ni elimina conceptos ya existentes.
    /// </summary>
    Task UpdateAsync(CancellationToken cancellationToken = default);
}
