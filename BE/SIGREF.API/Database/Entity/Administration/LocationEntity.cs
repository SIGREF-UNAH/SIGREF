using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SIGREF.API.Database.Entity.common;

namespace SIGREF.API.Database.Entity.Administration;


    [Table("locations")]
    public class LocationEntity : BaseEntity
    {
        // ============================
        //        DATOS PRINCIPALES
        // ============================

        [Required(ErrorMessage = "El nombre de la ubicación es obligatorio.")]
        [StringLength(150, ErrorMessage = "El nombre no puede exceder los 150 caracteres.")]
        [Column("name")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "El ID de FHIR Location es obligatorio.")]
        [StringLength(64, ErrorMessage = "El ID de FHIR no puede exceder los 64 caracteres.")]
        [Column("location_fhir_id")]
        public string LocationFHIR_ID { get; set; } = null!;
    }