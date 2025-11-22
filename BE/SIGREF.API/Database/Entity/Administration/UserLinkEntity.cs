using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SIGREF.API.Database.Entity.Administration;

[Table("user_links")]
public class UserLinkEntity
{
    // ============================
    //        PRIMARY KEY
    // ============================
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    // ============================
    //     KEYCLOAK IDENTIFIERS
    // ============================

    [Required(ErrorMessage = "El ID del usuario en Keycloak es obligatorio.")]
    [StringLength(64, ErrorMessage = "El ID de Keycloak no puede exceder los 64 caracteres.")]
    [Column("keycloak_user_id")]
    public string KeycloakUserId { get; set; } = null!;

    // ============================
    //     PRACTITIONER FHIR
    // ============================
    [Required(ErrorMessage = "El ID del Practitioner en FHIR es obligatorio.")]
    [StringLength(64, ErrorMessage = "El PractitionerId no puede exceder los 64 caracteres.")]
    [Column("practitioner_id")]
    public string PractitionerId { get; set; } = null!;

    // ============================
    //     SINCRONIZACIÓN
    // ============================

    [Column("last_sync")]
    public DateTime? LastSync { get; set; }

    [StringLength(32)]
    [Column("sync_status")]
    public string SyncStatus { get; set; } = "Pending";

    // ============================
    //     INFO ESPEJO DE KEYCLOAK
    // ============================

    [StringLength(256)]
    [Column("username")]
    public string? Username { get; set; }

    [StringLength(256)]
    [EmailAddress]
    [Column("email")]
    public string? Email { get; set; }

    [StringLength(256)]
    [Column("display_name")]
    public string? DisplayName { get; set; }

    // ============================
    //         STATUS
    // ============================
    [Column("is_active")]
    public bool Active { get; set; } = true;

    // ============================
    //         AUDITORÍA
    // ============================

    // Usuario que creó este UserLink (incluye SYSTEM)
    [Required(ErrorMessage = "El usuario que crea el registro es obligatorio.")]
    [Column("created_by_id")]
    public Guid? CreatedById { get; set; }

    [ForeignKey(nameof(CreatedById))]
    public UserLinkEntity? CreatedBy { get; set; } = null!;

    // Último usuario que lo modificó (puede ser null si nunca se modificó)
    [Column("updated_by_id")]
    public Guid? UpdatedById { get; set; }

    [ForeignKey(nameof(UpdatedById))]
    public UserLinkEntity? UpdatedBy { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}