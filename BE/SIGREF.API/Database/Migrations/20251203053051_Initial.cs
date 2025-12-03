using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIGREF.API.Database.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "health_services",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    health_service_id_fhir = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, comment: "ID del recurso HealthcareService en FHIR."),
                    price = table.Column<decimal>(type: "numeric", nullable: false, comment: "Precio asignado al servicio para facturación."),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_by_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Usuario que creó el registro."),
                    updated_by_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Fecha de creación (UTC)."),
                    updated_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Fecha de última actualización (UTC).")
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
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
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
                    currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    is_singleton = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_by_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "ID del usuario que creó el registro."),
                    updated_by_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "ID del usuario que realizó la última actualización."),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Fecha de creación del turno (UTC)."),
                    updated_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Fecha de última actualización (UTC).")
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
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    prefix = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    start_number = table.Column<long>(type: "bigint", nullable: false),
                    end_number = table.Column<long>(type: "bigint", nullable: false),
                    current_number = table.Column<long>(type: "bigint", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_by_id = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_by_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_invoice_serie", x => x.id);
                });

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
                    media_type = table.Column<int>(type: "integer", nullable: false, comment: "Tipo lógico del archivo (AppHospital, HealthGuilt)."),
                    size_bytes = table.Column<long>(type: "bigint", nullable: false, comment: "Tamaño del archivo en bytes."),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_by_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "ID del usuario que creó el registro."),
                    updated_by_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "ID del usuario que realizó la última actualización."),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Fecha de creación del turno (UTC)."),
                    updated_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Fecha de última actualización (UTC).")
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
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sql_query = table.Column<string>(type: "text", nullable: false),
                    hospital_properties_snapshot = table.Column<string>(type: "text", nullable: false),
                    display = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    system_display = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    format = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_by_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "ID del usuario que creó el registro."),
                    updated_by_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "ID del usuario que realizó la última actualización."),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Fecha de creación del turno (UTC)."),
                    updated_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Fecha de última actualización (UTC).")
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
                    correction_closure = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, comment: "Indica si el turno está activo."),
                    created_by_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "ID del usuario que creó el registro."),
                    updated_by_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "ID del usuario que realizó la última actualización."),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Fecha de creación del turno (UTC)."),
                    updated_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Fecha de última actualización (UTC).")
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
                    open_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Fecha y hora exacta en la que el cajero abrió la sesión de caja."),
                    closed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Fecha y hora en la que se cerró la sesión de caja."),
                    declared_amount = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: true, comment: "Monto declarado por el cajero al momento del cierre."),
                    system_amount = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: true, comment: "Monto calculado automáticamente por el sistema según los recibos generados."),
                    difference = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: true, comment: "Diferencia entre el monto declarado por el cajero y el monto calculado por el sistema."),
                    is_open = table.Column<bool>(type: "boolean", nullable: false, comment: "Indica si la sesión está activa (abierta)."),
                    requires_correction = table.Column<bool>(type: "boolean", nullable: false, comment: "Indica si la sesión requiere corrección debido a una diferencia detectada."),
                    correction_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Fecha en la que la sesión fue revisada/corregida por un administrador o auditor."),
                    notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true, comment: "Notas o comentarios del cajero o administrador sobre discrepancias o correcciones."),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_by_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "ID del usuario que creó el registro."),
                    updated_by_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "ID del usuario que realizó la última actualización."),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Fecha de creación del turno (UTC)."),
                    updated_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Fecha de última actualización (UTC).")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cashier_sessions", x => x.id);
                    table.ForeignKey(
                        name: "FK_cashier_sessions_shifts_shift_id",
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
                    patient_id_fhir = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    patient_display = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    patient_system = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    patient_value = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    service_group_fhir_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    single_service_id = table.Column<Guid>(type: "uuid", nullable: true),
                    serie_id = table.Column<Guid>(type: "uuid", nullable: false),
                    number = table.Column<long>(type: "bigint", nullable: false),
                    total_original = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    adjustment_total = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    final_total = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    amount_paid = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    amount_due = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    status = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false, comment: "Created: Creada | Paid: pagada | Cancelled: anulada | Refunded: reembolsada"),
                    invoice_type = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false, comment: "Normal: Todos Datos | Emergency: Se reconoce Servicio Dado Datos pueden quedar pendientes | Exempt: Descuento del 100% | Refunded: reembolsada | CreditNote: Devolucion de Dinero | DebitNote: Ingreso de Dinero"),
                    payment_method = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, comment: "Método de pago: Cash, Card, Transfer, Mixed"),
                    parent_invoice_id = table.Column<Guid>(type: "uuid", nullable: true),
                    cashier_session_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_by_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "ID del usuario que creó el registro."),
                    updated_by_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "ID del usuario que realizó la última actualización."),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Fecha de creación del turno (UTC)."),
                    updated_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Fecha de última actualización (UTC).")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_invoices", x => x.id);
                    table.ForeignKey(
                        name: "FK_invoices_cashier_sessions_cashier_session_id",
                        column: x => x.cashier_session_id,
                        principalTable: "cashier_sessions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_invoices_invoice_serie_serie_id",
                        column: x => x.serie_id,
                        principalTable: "invoice_serie",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_invoices_invoices_parent_invoice_id",
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
                    service_id = table.Column<Guid>(type: "uuid", nullable: false),
                    description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, comment: "Nombre del servicio copiado al momento de facturar (histórico)."),
                    quantity = table.Column<int>(type: "integer", nullable: false, comment: "Cantidad facturada del servicio."),
                    unit_price = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false, comment: "Precio unitario histórico del servicio facturado."),
                    discount = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: true, comment: "Descuento aplicado al item (si aplica)."),
                    total_amount = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false, comment: "Total del item: (quantity * unit_price) - discount (congelado)."),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_by_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Usuario que creó el item."),
                    updated_by_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Usuario que actualizó el item (si aplica)."),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Fecha de creación del item (UTC)."),
                    updated_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Fecha de última actualización del item (UTC).")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_invoice_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_invoice_items_health_services_service_id",
                        column: x => x.service_id,
                        principalTable: "health_services",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_invoice_items_invoices_invoice_id",
                        column: x => x.invoice_id,
                        principalTable: "invoices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Items facturados: cada servicio congelado con precio histórico.");

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
                name: "idx_cashier_sessions_user",
                table: "cashier_sessions",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "uq_cashier_sessions_user_open",
                table: "cashier_sessions",
                columns: new[] { "user_id", "is_open" },
                unique: true);

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
                column: "created_by_user_id");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "hospital_properties");

            migrationBuilder.DropTable(
                name: "invoice_items");

            migrationBuilder.DropTable(
                name: "media_files");

            migrationBuilder.DropTable(
                name: "report_history");

            migrationBuilder.DropTable(
                name: "health_services");

            migrationBuilder.DropTable(
                name: "invoices");

            migrationBuilder.DropTable(
                name: "cashier_sessions");

            migrationBuilder.DropTable(
                name: "invoice_serie");

            migrationBuilder.DropTable(
                name: "shifts");
        }
    }
}
