using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIGREF.API.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddDashboardMaterializedView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ============================================
            // 1. Crear la MATERIALIZED VIEW optimizada
            // ============================================
            migrationBuilder.Sql(@"
                CREATE MATERIALIZED VIEW IF NOT EXISTS mv_dashboard_facts AS
                SELECT
                    -- Identificadores
                    i.id                                     AS invoice_id,
                    it.id                                    AS item_id,

                    -- Tiempo
                    i.created_date                           AS created_date,

                    -- Totales
                    i.final_total                            AS final_total,

                    -- Income real (considerando notas)
                    CASE 
                        WHEN i.invoice_type = 'CreditNote' THEN -i.final_total
                        WHEN i.invoice_type = 'Exempt' THEN 0
                        WHEN i.invoice_type = 'Cancelled' THEN 0
                        WHEN i.invoice_type = 'Refunded' THEN 0
                        ELSE i.final_total
                    END AS real_income,

                    -- Tipo y estado
                    i.invoice_type                           AS invoice_type,
                    i.status                                 AS status,

                    -- Paciente
                    i.patient_id_fhir                        AS patient_id_fhir,

                    -- Shift & Location
                    cs.shift_id                              AS shift_id,
                    s.name                                   AS shift_name,
                    s.location_id                            AS location_id,

                    -- Servicio / Paquete
                    it.service_id                            AS service_id,
                    i.service_group_fhir_id                  AS package_id,

                    -- Contadores
                    CASE WHEN it.id IS NOT NULL THEN 1 ELSE 0 END AS total_items,

                    -- Cashier Session
                    cs.id                                    AS cashier_session_id,
                    cs.created_by_id                         AS cashier_user_id


                FROM invoices i
                LEFT JOIN cashier_sessions cs ON cs.id = i.cashier_session_id
                LEFT JOIN shifts s ON s.id = cs.shift_id

                LEFT JOIN invoice_items it ON it.invoice_id = i.id
                LEFT JOIN health_services hs ON hs.id = it.service_id

                WHERE i.is_active = TRUE;
            ");

            // ============================================
            // 2. Crear índices optimizados
            // ============================================

            // Índices por fecha
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS idx_mv_facts_created_date ON mv_dashboard_facts(created_date);");

            // Índices de filtros comunes
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS idx_mv_facts_status ON mv_dashboard_facts(status);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS idx_mv_facts_type ON mv_dashboard_facts(invoice_type);");

            // Índices para dimensiones del dashboard
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS idx_mv_facts_shift ON mv_dashboard_facts(shift_id);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS idx_mv_facts_location ON mv_dashboard_facts(location_id);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS idx_mv_facts_service ON mv_dashboard_facts(service_id);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS idx_mv_facts_package ON mv_dashboard_facts(package_id);");

            // Compuesto por fecha + estado
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS idx_mv_facts_created_status ON mv_dashboard_facts(created_date, status);");

            // Compuesto por fecha + location
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS idx_mv_facts_created_location ON mv_dashboard_facts(created_date, location_id);");

            // Ingresos reales (para agregaciones rápidas)
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS idx_mv_facts_real_income ON mv_dashboard_facts(real_income);");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP MATERIALIZED VIEW IF EXISTS mv_dashboard_facts;");
        }
    }
}
