using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SIGREF.API.Database.Entity;

[Table("user_links")]
public class UserLink
{
    // me estuvo marcado un error , una advertencia y encontre esto
    /// <summary>
    /// https://stackoverflow.com/questions/55525861/c8-what-does-default-do-on-generic-types
    /// </summary>
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    //ID del usuario en Keycloak (claim "sub" del JWT)
    [Required]
    [Column("keycloak_user_id")]
    [MaxLength(64)]
    public string KeycloakUserId { get; set; } = default!;

    //ID del Practitioner en HAPI FHIR
    [Required]
    [Column("practitioner_id")]
    [MaxLength(64)]
    public string PractitionerId { get; set; } = default!;

    // Control de sincronización
    [Column("last_sync")]
    public DateTime? LastSync { get; set; }
    
    
    // Datos espejo de Keycloak 
    [Column("username")]
    public string? Username { get; set; }

    [Column("email")]
    [MaxLength(256)]
    [EmailAddress]
    public string? Email { get; set; }
    
    // Para busquedas rapidas
    [Column("display_name")]
    [MaxLength(256)]
    public string? DisplayName { get; set; } // Nombre del practitioner
    
    [Column("sync_status")]
    [MaxLength(32)]
    public string SyncStatus { get; set; } = "Pending";

    // Estado general (activo / inactivo)
    [Column("active")]
    public bool Active { get; set; } = true;

    // Control de auditoria
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}