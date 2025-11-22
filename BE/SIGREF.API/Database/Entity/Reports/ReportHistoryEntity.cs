using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SIGREF.API.Database.Entity.Administration;
using SIGREF.API.Database.Entity.common;

namespace SIGREF.API.Database.Entity.Reports;

[Table("report_history")]
public class ReportHistoryEntity : BaseEntity
{
    // ===============================
    //        TIPO DE REPORTE
    // ===============================

    [Required]
    [StringLength(100)]
    [Column("report_type")]
    public string ReportType { get; set; } = null!;
    // Ej: "CashierSummary", "InvoiceDetail", "DailyClose", "AuditTrail"


    // ===============================
    //        USUARIO CREADOR
    // ===============================

    [Required]
    [Column("created_by_user_id")]
    public Guid CreatedByUserId { get; set; }

    [ForeignKey(nameof(CreatedByUserId))] public UserLinkEntity? CreatedByUser { get; set; }


    // ===============================
    //          SQL GENERADO
    // ===============================

    [Required] [Column("sql_query")] public string SqlQuery { get; set; } = null!;
    // Guarda la consulta del reporte generada dinámicamente


    // ===============================
    //   SNAPSHOT HOSPITAL PROPERTIES
    // ===============================

    [Required]
    [Column("hospital_properties_snapshot")]
    public string HospitalPropertiesSnapshot { get; set; } = null!;
    // Aquí se guarda JSON literal:
    // { "Name": "...", "Director": "...", ... }
    // NO es FK, NO es relación


    // ===============================
    //   DISPLAY / DESCRIPCIÓN
    // ===============================

    [StringLength(300)]
    [Column("display")]
    public string? Display { get; set; }

    [StringLength(300)]
    [Column("system_display")]
    public string? DisplaySystem { get; set; }


    // ===============================
    //       FORMATO GENERADO
    // ===============================

    [Required]
    [StringLength(20)]
    [Column("format")]
    public string Format { get; set; } = "PDF";
}