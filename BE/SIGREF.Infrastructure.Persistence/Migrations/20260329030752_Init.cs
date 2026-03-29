using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIGREF.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "health_services",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    health_service_id_fhir = table.Column<string>(type: "character varying(64)", unicode: false, maxLength: 64, nullable: false, comment: "ID lógico del recurso HealthcareService según estándar FHIR R4 (max 64 chars)."),
                    price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, comment: "Precio asignado al servicio para facturación (máximo 18 dígitos, 2 decimales)."),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_by_id = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_by_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_health_services", x => x.id);
                },
                comment: "Catálogo de servicios de salud registrados en SIGREF. Sincronizado parcialmente con FHIR HealthcareService.");

            migrationBuilder.CreateTable(
                name: "hospital_properties",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, comment: "Nombre oficial del hospital"),
                    director = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    subdirector = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    location = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    logo_media_id = table.Column<Guid>(type: "uuid", nullable: true),
                    url_logo = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    health_logo_media_id = table.Column<Guid>(type: "uuid", nullable: true),
                    url_logo_health = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    phone_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    hospital_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    rtn = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    website = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    is_singleton = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_by_id = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_by_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hospital_properties", x => x.id);
                },
                comment: "Propiedades generales del hospital: datos administrativos, logos, contacto y configuración base. Tabla singleton.");

            migrationBuilder.CreateTable(
                name: "invoice_serie",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false, comment: "Nombre descriptivo de la serie (ej: Serie A - Principal)"),
                    prefix = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, comment: "Prefijo de la factura (ej: F001, A)"),
                    start_number = table.Column<long>(type: "bigint", nullable: false, comment: "Número inicial autorizado para esta serie"),
                    end_number = table.Column<long>(type: "bigint", nullable: false, comment: "Número final autorizado para esta serie"),
                    current_number = table.Column<long>(type: "bigint", nullable: false, comment: "Último número de factura emitido en esta serie"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_by_id = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_by_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_invoice_serie", x => x.id);
                },
                comment: "Tabla para gestionar las series de facturación y el control de su correlativo actual.");

            migrationBuilder.CreateTable(
                name: "media_files",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    file_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "Nombre original del archivo subido."),
                    content_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, comment: "Tipo MIME real del archivo (image/png, image/jpeg)."),
                    relative_path = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false, comment: "Ruta relativa pública del archivo: /media/..."),
                    description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true, comment: "Descripción asignada por el usuario."),
                    system_description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true, comment: "Descripción del archivo generada por el sistema."),
                    media_type = table.Column<string>(type: "text", nullable: false, comment: "Tipo lógico del archivo (AppHospital, HealthGuilt)."),
                    size_bytes = table.Column<long>(type: "bigint", nullable: false, comment: "Tamaño del archivo en bytes."),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_by_id = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_by_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_media_files", x => x.id);
                },
                comment: "Archivos multimedia almacenados en el sistema (logos, imágenes varias).");

            migrationBuilder.CreateTable(
                name: "report_history",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    report_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    requested_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    hangfire_job_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    status = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, comment: "Estado del Reporte : Pending | Processing | Completed | Failed "),
                    progress = table.Column<int>(type: "integer", maxLength: 3, nullable: false),
                    download_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    period_label = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    sql_query = table.Column<string>(type: "text", nullable: false),
                    hospital_properties_configuration = table.Column<string>(type: "jsonb", nullable: false),
                    error_message = table.Column<string>(type: "text", nullable: true),
                    filter = table.Column<string>(type: "jsonb", nullable: true, comment: "JSON Filter del reporte del hospital."),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_by_id = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_by_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_report_history", x => x.id);
                },
                comment: "Historial de reportes ejecutados en SIGREF. Guarda SQL generado, tipo de reporte, usuario ejecutor, formato y snapshot del hospital.");

            migrationBuilder.CreateTable(
                name: "shifts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    location_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, comment: "ID de la Location en FHIR asociada a este turno."),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false, comment: "Nombre del turno (ejemplo: 'Matutino', 'Vespertino', etc.)."),
                    start_time = table.Column<TimeOnly>(type: "time without time zone", nullable: false, comment: "Hora de inicio del turno (TimeOnly)."),
                    end_time = table.Column<TimeOnly>(type: "time without time zone", nullable: false, comment: "Hora de finalización del turno (TimeOnly)."),
                    correction_closure = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true, comment: "Indica si el turno está activo."),
                    created_by_id = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_by_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shifts", x => x.id);
                },
                comment: "Catálogo de turnos asignados a ubicaciones del hospital (Location - FHIR).");

            migrationBuilder.CreateTable(
                name: "cashier_sessions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Id del usuario (Keycloak) al que pertenece la sesión de caja."),
                    shift_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Turno asignado a esta sesión de caja."),
                    open_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "Fecha y hora exacta en la que el cajero abrió la sesión de caja."),
                    closed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "Fecha y hora en la que se cerró la sesión de caja."),
                    declared_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true, comment: "Monto declarado por el cajero al momento del cierre."),
                    system_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true, comment: "Monto calculado automáticamente por el sistema según los recibos generados."),
                    difference = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true, comment: "Diferencia entre el monto declarado por el cajero y el monto calculado por el sistema."),
                    is_open = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true, comment: "Indica si la sesión está activa (abierta)."),
                    requires_correction = table.Column<bool>(type: "boolean", nullable: false, comment: "Indica si la sesión requiere corrección debido a una diferencia detectada."),
                    correction_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "Fecha en la que la sesión fue revisada/corregida por un administrador o auditor."),
                    notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true, comment: "Notas o comentarios del cajero o administrador sobre discrepancias o correcciones."),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_by_id = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_by_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cashier_sessions", x => x.id);
                    table.ForeignKey(
                        name: "fk_shift_id",
                        column: x => x.shift_id,
                        principalTable: "shifts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Tabla que almacena las sesiones de caja por usuario, incluyendo montos, diferencias y estado del arqueo.");

            migrationBuilder.CreateTable(
                name: "invoices",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    patient_id_fhir = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true, comment: "ID único del paciente en el servidor externo FHIR."),
                    patient_display = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true, comment: "Nombre o alias del paciente al momento de facturar."),
                    patient_system = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, comment: "Namespace del sistema de identificación (ej: URL de identidad)."),
                    patient_value = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "Valor del documento de identidad (DNI/Pasaporte)."),
                    service_group_fhir_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    single_service_id = table.Column<Guid>(type: "uuid", nullable: true),
                    serie_id = table.Column<Guid>(type: "uuid", nullable: false),
                    number = table.Column<long>(type: "bigint", nullable: false),
                    total_original = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false, comment: "Monto bruto total (Suma de items)."),
                    invoice_discount = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false, comment: "Descuento total aplicado a la factura en su creación."),
                    adjustment_total = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false, comment: "Suma neta de ajustes por notas de crédito/débito."),
                    final_total = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false, comment: "Total exigible (TotalOriginal - Discount + Adjustment)."),
                    amount_paid = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false, comment: "Monto efectivamente cobrado."),
                    amount_due = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false, comment: "Monto pendiente de cobro."),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, comment: "Created: Creada | Paid: Pagada | Cancelled: Anulada | Refunded: Reembolsada"),
                    invoice_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, comment: "Tipo legal: Normal, Emergency, Exempt, CreditNote, DebitNote."),
                    payment_method = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, comment: "Método de pago: Cash, Card, Transfer, Mixed."),
                    parent_invoice_id = table.Column<Guid>(type: "uuid", nullable: true),
                    cashier_session_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_by_id = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_by_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_invoices", x => x.id);
                    table.ForeignKey(
                        name: "fk_cashier_session_id",
                        column: x => x.cashier_session_id,
                        principalTable: "cashier_sessions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_invoice_serie_id",
                        column: x => x.serie_id,
                        principalTable: "invoice_serie",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_parent_invoice_id",
                        column: x => x.parent_invoice_id,
                        principalTable: "invoices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Tabla principal de facturación: contiene facturas normales, emergencias, exentas y notas de crédito/débito.");

            migrationBuilder.CreateTable(
                name: "invoice_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    invoice_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fk_invoice_items_invoice = table.Column<Guid>(type: "uuid", nullable: false),
                    service_id = table.Column<Guid>(type: "uuid", nullable: false),
                    description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, comment: "Nombre del servicio copiado al momento de facturar (congelado)."),
                    quantity = table.Column<int>(type: "integer", nullable: false, comment: "Cantidad facturada de este ítem."),
                    unit_price = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false, comment: "Precio unitario histórico del servicio al momento de la venta."),
                    total_amount = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false, comment: "Total de la línea: (Quantity * UnitPrice). No incluye descuentos."),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_by_id = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_by_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_invoice_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_invoice_items_id",
                        column: x => x.fk_invoice_items_invoice,
                        principalTable: "invoices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_invoice_items_service",
                        column: x => x.service_id,
                        principalTable: "health_services",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Items facturados: cada servicio congelado con precio histórico.");

            // [REF: Issue #259 | Commit a5a3f4e]
            // [FILE: BE/SIGREF.Infrastructure.Persistence/Configurations/CashierSessionConfiguration.cs]
            // ---------------------------------------------------------------------------------
            // NOTA DE INFRAESTRUCTURA: PostgreSQL + EF Core Indexing
            // Debido a limitaciones de EF Core con índices múltiples en la misma columna (user_id),
            // estos índices se definen manualmente en esta migración para evitar conflictos 
            // con el Model Snapshot. Cualquier cambio aquí debe reflejarse en la clase de 
            // configuración mencionada arriba.
            // ---------------------------------------------------------------------------------
            migrationBuilder.CreateIndex(
                name: "idx_cashier_sessions_user",
                table:"cashier_sessions",
                column: "user_id");
            
            migrationBuilder.CreateIndex(
                name: "idx_cashier_sessions_closed_at",
                table: "cashier_sessions",
                column: "closed_at");

            migrationBuilder.CreateIndex(
                name: "idx_cashier_sessions_is_open",
                table: "cashier_sessions",
                column: "is_open");

            migrationBuilder.CreateIndex(
                name: "idx_cashier_sessions_open_at",
                table: "cashier_sessions",
                column: "open_at");

            migrationBuilder.CreateIndex(
                name: "idx_cashier_sessions_requires_correction",
                table: "cashier_sessions",
                column: "requires_correction");

            migrationBuilder.CreateIndex(
                name: "idx_cashier_sessions_shift",
                table: "cashier_sessions",
                column: "shift_id");

            migrationBuilder.CreateIndex(
                name: "uq_cashier_sessions_user_open",
                table: "cashier_sessions",
                column: "user_id",
                unique: true,
                filter: "is_open = true");

            migrationBuilder.CreateIndex(
                name: "idx_health_services_fhir",
                table: "health_services",
                column: "health_service_id_fhir",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_singleton_enforcer",
                table: "hospital_properties",
                column: "is_singleton",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_invoiceitems_created_date",
                table: "invoice_items",
                column: "created_date");

            migrationBuilder.CreateIndex(
                name: "idx_invoiceitems_invoiceid",
                table: "invoice_items",
                column: "invoice_id");

            migrationBuilder.CreateIndex(
                name: "idx_invoiceitems_invoiceid_serviceid",
                table: "invoice_items",
                columns: new[] { "invoice_id", "service_id" });

            migrationBuilder.CreateIndex(
                name: "idx_invoiceitems_serviceid",
                table: "invoice_items",
                column: "service_id");

            migrationBuilder.CreateIndex(
                name: "idx_invoiceitems_serviceid_created",
                table: "invoice_items",
                columns: new[] { "service_id", "created_date" });

            migrationBuilder.CreateIndex(
                name: "IX_invoice_items_fk_invoice_items_invoice",
                table: "invoice_items",
                column: "fk_invoice_items_invoice");

            migrationBuilder.CreateIndex(
                name: "idx_invoice_serie_name",
                table: "invoice_serie",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "idx_invoice_serie_prefix",
                table: "invoice_serie",
                column: "prefix",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_invoice_cashier_sesion",
                table: "invoices",
                column: "cashier_session_id");

            migrationBuilder.CreateIndex(
                name: "idx_invoice_created_by",
                table: "invoices",
                column: "created_by_id");

            migrationBuilder.CreateIndex(
                name: "idx_invoice_created_date",
                table: "invoices",
                column: "created_date");

            migrationBuilder.CreateIndex(
                name: "idx_invoice_created_status_type",
                table: "invoices",
                columns: new[] { "created_date", "status", "invoice_type" });

            migrationBuilder.CreateIndex(
                name: "idx_invoice_parent",
                table: "invoices",
                column: "parent_invoice_id");

            migrationBuilder.CreateIndex(
                name: "idx_invoice_patient_id",
                table: "invoices",
                column: "patient_id_fhir");

            migrationBuilder.CreateIndex(
                name: "idx_invoice_serie",
                table: "invoices",
                column: "serie_id");

            migrationBuilder.CreateIndex(
                name: "idx_invoice_serie_number",
                table: "invoices",
                columns: new[] { "serie_id", "number" });

            migrationBuilder.CreateIndex(
                name: "idx_invoice_status",
                table: "invoices",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "idx_invoice_type",
                table: "invoices",
                column: "invoice_type");

            migrationBuilder.CreateIndex(
                name: "idx_media_files_created_date",
                table: "media_files",
                column: "created_date");

            migrationBuilder.CreateIndex(
                name: "idx_media_files_filename",
                table: "media_files",
                column: "file_name");

            migrationBuilder.CreateIndex(
                name: "idx_media_files_type",
                table: "media_files",
                column: "media_type");

            migrationBuilder.CreateIndex(
                name: "idx_report_history_type",
                table: "report_history",
                column: "report_type");

            migrationBuilder.CreateIndex(
                name: "idx_report_history_user",
                table: "report_history",
                column: "created_by_id");

            migrationBuilder.CreateIndex(
                name: "idx_shifts_location",
                table: "shifts",
                column: "location_id");

            migrationBuilder.CreateIndex(
                name: "idx_shifts_name",
                table: "shifts",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "idx_shifts_name_location",
                table: "shifts",
                columns: new[] { "name", "location_id" });
            
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
                    hs.health_service_id_fhir                AS health_service_id_fhir,
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
            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS idx_mv_facts_created_location_shift
                ON public.mv_dashboard_facts (created_date, location_id, shift_id);
                ");

            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS idx_mv_facts_invoice
                ON public.mv_dashboard_facts (invoice_id);
                ");

            // Índices por fecha
            migrationBuilder.Sql(@"CREATE INDEX IF NOT EXISTS idx_mv_facts_created_date ON mv_dashboard_facts(created_date);");

            // Índices de filtros comunes
            migrationBuilder.Sql(@"CREATE INDEX IF NOT EXISTS idx_mv_facts_status ON mv_dashboard_facts(status);");
            migrationBuilder.Sql(@"CREATE INDEX IF NOT EXISTS idx_mv_facts_type ON mv_dashboard_facts(invoice_type);");

            // Índices para dimensiones del dashboard
            migrationBuilder.Sql(@"CREATE INDEX IF NOT EXISTS idx_mv_facts_shift ON mv_dashboard_facts(shift_id);");
            migrationBuilder.Sql(@"CREATE INDEX IF NOT EXISTS idx_mv_facts_location ON mv_dashboard_facts(location_id);");
            migrationBuilder.Sql(@"CREATE INDEX IF NOT EXISTS idx_mv_facts_service ON mv_dashboard_facts(service_id);");
            migrationBuilder.Sql(@"CREATE INDEX IF NOT EXISTS idx_mv_facts_package ON mv_dashboard_facts(package_id);");

            // Compuesto por fecha + estado
            migrationBuilder.Sql(@"CREATE INDEX IF NOT EXISTS idx_mv_facts_created_status ON mv_dashboard_facts(created_date, status);");

            // Compuesto por fecha + location
            migrationBuilder.Sql(@"CREATE INDEX IF NOT EXISTS idx_mv_facts_created_location ON mv_dashboard_facts(created_date, location_id);");

            // Ingresos reales (para agregaciones rápidas)
            migrationBuilder.Sql(@"CREATE INDEX IF NOT EXISTS idx_mv_facts_real_income ON mv_dashboard_facts(real_income);");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP MATERIALIZED VIEW IF EXISTS mv_dashboard_facts;");

            migrationBuilder.DropTable(
                name: "hospital_properties");

            migrationBuilder.DropTable(
                name: "invoice_items");

            migrationBuilder.DropTable(
                name: "media_files");

            migrationBuilder.DropTable(
                name: "report_history");

            migrationBuilder.DropTable(
                name: "invoices");

            migrationBuilder.DropTable(
                name: "health_services");

            migrationBuilder.DropTable(
                name: "cashier_sessions");

            migrationBuilder.DropTable(
                name: "invoice_serie");

            migrationBuilder.DropTable(
                name: "shifts");
        }
    }
}
