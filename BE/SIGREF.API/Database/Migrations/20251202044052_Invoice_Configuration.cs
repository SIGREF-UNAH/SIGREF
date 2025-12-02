using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIGREF.API.Database.Migrations
{
    /// <inheritdoc />
    public partial class Invoice_Configuration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_invoice_items_health_services_service_id",
                table: "invoice_items");

            migrationBuilder.DropIndex(
                name: "idx_invoice_number",
                table: "invoices");

            migrationBuilder.DropIndex(
                name: "idx_invoice_user",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "currency",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "total_amount",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "user_id",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "package_id",
                table: "invoice_items");

            migrationBuilder.RenameIndex(
                name: "IX_invoices_parent_invoice_id",
                table: "invoices",
                newName: "idx_invoice_parent");

            migrationBuilder.RenameIndex(
                name: "idx_invoice_session",
                table: "invoices",
                newName: "idx_invoice_cashier_sesion");

            migrationBuilder.RenameIndex(
                name: "IX_invoice_items_service_id",
                table: "invoice_items",
                newName: "idx_invoiceitems_serviceid");

            migrationBuilder.RenameIndex(
                name: "IX_invoice_items_invoice_id",
                table: "invoice_items",
                newName: "idx_invoiceitems_invoiceid");

            migrationBuilder.AlterTable(
                name: "invoices",
                comment: "Tabla principal de facturación: contiene facturas normales, emergencias, exentas y notas de crédito/débito.",
                oldComment: "Ordenes de Donacion emitidas por SIGREF, con información FHIR del paciente, series, métodos de pago y relaciones administrativas.");

            migrationBuilder.AlterTable(
                name: "invoice_items",
                comment: "Items facturados: cada servicio congelado con precio histórico.");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "invoices",
                type: "varchar(30)",
                maxLength: 30,
                nullable: false,
                comment: "Created: Creada | Paid: pagada | Cancelled: anulada | Refunded: reembolsada",
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<int>(
                name: "payment_method",
                table: "invoices",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "invoice_type",
                table: "invoices",
                type: "varchar(30)",
                maxLength: 30,
                nullable: false,
                comment: "Normal: Todos Datos | Emergency: Se reconoce Servicio Dado Datos pueden quedar pendientes | Exempt: Descuento del 100% | Refunded: reembolsada | CreditNote: Devolucion de Dinero | DebitNote: Ingreso de Dinero",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AddColumn<decimal>(
                name: "adjustment_total",
                table: "invoices",
                type: "numeric(14,2)",
                precision: 14,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "amount_due",
                table: "invoices",
                type: "numeric(14,2)",
                precision: 14,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "amount_paid",
                table: "invoices",
                type: "numeric(14,2)",
                precision: 14,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "final_total",
                table: "invoices",
                type: "numeric(14,2)",
                precision: 14,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "service_group_fhir_id",
                table: "invoices",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "single_service_id",
                table: "invoices",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "total_original",
                table: "invoices",
                type: "numeric(14,2)",
                precision: 14,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_date",
                table: "invoice_items",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Fecha de última actualización del item (UTC).",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "updated_by_id",
                table: "invoice_items",
                type: "uuid",
                nullable: true,
                comment: "Usuario que actualizó el item (si aplica).",
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "unit_price",
                table: "invoice_items",
                type: "numeric(14,2)",
                precision: 14,
                scale: 2,
                nullable: false,
                comment: "Precio unitario histórico del servicio facturado.",
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<Guid>(
                name: "service_id",
                table: "invoice_items",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "quantity",
                table: "invoice_items",
                type: "integer",
                nullable: false,
                comment: "Cantidad facturada del servicio.",
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<decimal>(
                name: "discount",
                table: "invoice_items",
                type: "numeric(14,2)",
                precision: 14,
                scale: 2,
                nullable: true,
                comment: "Descuento aplicado al item (si aplica).",
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_date",
                table: "invoice_items",
                type: "timestamp with time zone",
                nullable: false,
                comment: "Fecha de creación del item (UTC).",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<Guid>(
                name: "created_by_id",
                table: "invoice_items",
                type: "uuid",
                nullable: false,
                comment: "Usuario que creó el item.",
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "invoice_items",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                comment: "Nombre del servicio copiado al momento de facturar (histórico).");

            migrationBuilder.AddColumn<decimal>(
                name: "total_amount",
                table: "invoice_items",
                type: "numeric(14,2)",
                precision: 14,
                scale: 2,
                nullable: false,
                defaultValue: 0m,
                comment: "Total del item: (quantity * unit_price) - discount (congelado).");

            migrationBuilder.CreateIndex(
                name: "idx_invoice_created_by",
                table: "invoices",
                column: "created_by_id");

            migrationBuilder.CreateIndex(
                name: "idx_invoice_created_date",
                table: "invoices",
                column: "created_date");

            migrationBuilder.CreateIndex(
                name: "idx_invoice_patient_id",
                table: "invoices",
                column: "patient_id_fhir");

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
                name: "idx_invoiceitems_invoiceid_serviceid",
                table: "invoice_items",
                columns: new[] { "invoice_id", "service_id" });

            migrationBuilder.AddForeignKey(
                name: "FK_invoice_items_health_services_service_id",
                table: "invoice_items",
                column: "service_id",
                principalTable: "health_services",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_invoice_items_health_services_service_id",
                table: "invoice_items");

            migrationBuilder.DropIndex(
                name: "idx_invoice_created_by",
                table: "invoices");

            migrationBuilder.DropIndex(
                name: "idx_invoice_created_date",
                table: "invoices");

            migrationBuilder.DropIndex(
                name: "idx_invoice_patient_id",
                table: "invoices");

            migrationBuilder.DropIndex(
                name: "idx_invoice_serie_number",
                table: "invoices");

            migrationBuilder.DropIndex(
                name: "idx_invoice_status",
                table: "invoices");

            migrationBuilder.DropIndex(
                name: "idx_invoice_type",
                table: "invoices");

            migrationBuilder.DropIndex(
                name: "idx_invoiceitems_invoiceid_serviceid",
                table: "invoice_items");

            migrationBuilder.DropColumn(
                name: "adjustment_total",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "amount_due",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "amount_paid",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "final_total",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "service_group_fhir_id",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "single_service_id",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "total_original",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "description",
                table: "invoice_items");

            migrationBuilder.DropColumn(
                name: "total_amount",
                table: "invoice_items");

            migrationBuilder.RenameIndex(
                name: "idx_invoice_parent",
                table: "invoices",
                newName: "IX_invoices_parent_invoice_id");

            migrationBuilder.RenameIndex(
                name: "idx_invoice_cashier_sesion",
                table: "invoices",
                newName: "idx_invoice_session");

            migrationBuilder.RenameIndex(
                name: "idx_invoiceitems_serviceid",
                table: "invoice_items",
                newName: "IX_invoice_items_service_id");

            migrationBuilder.RenameIndex(
                name: "idx_invoiceitems_invoiceid",
                table: "invoice_items",
                newName: "IX_invoice_items_invoice_id");

            migrationBuilder.AlterTable(
                name: "invoices",
                comment: "Ordenes de Donacion emitidas por SIGREF, con información FHIR del paciente, series, métodos de pago y relaciones administrativas.",
                oldComment: "Tabla principal de facturación: contiene facturas normales, emergencias, exentas y notas de crédito/débito.");

            migrationBuilder.AlterTable(
                name: "invoice_items",
                oldComment: "Items facturados: cada servicio congelado con precio histórico.");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "invoices",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(30)",
                oldMaxLength: 30,
                oldComment: "Created: Creada | Paid: pagada | Cancelled: anulada | Refunded: reembolsada");

            migrationBuilder.AlterColumn<string>(
                name: "payment_method",
                table: "invoices",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "invoice_type",
                table: "invoices",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(30)",
                oldMaxLength: 30,
                oldComment: "Normal: Todos Datos | Emergency: Se reconoce Servicio Dado Datos pueden quedar pendientes | Exempt: Descuento del 100% | Refunded: reembolsada | CreditNote: Devolucion de Dinero | DebitNote: Ingreso de Dinero");

            migrationBuilder.AddColumn<string>(
                name: "currency",
                table: "invoices",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "total_amount",
                table: "invoices",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "user_id",
                table: "invoices",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_date",
                table: "invoice_items",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldComment: "Fecha de última actualización del item (UTC).");

            migrationBuilder.AlterColumn<Guid>(
                name: "updated_by_id",
                table: "invoice_items",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true,
                oldComment: "Usuario que actualizó el item (si aplica).");

            migrationBuilder.AlterColumn<decimal>(
                name: "unit_price",
                table: "invoice_items",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(14,2)",
                oldPrecision: 14,
                oldScale: 2,
                oldComment: "Precio unitario histórico del servicio facturado.");

            migrationBuilder.AlterColumn<Guid>(
                name: "service_id",
                table: "invoice_items",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<int>(
                name: "quantity",
                table: "invoice_items",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "Cantidad facturada del servicio.");

            migrationBuilder.AlterColumn<decimal>(
                name: "discount",
                table: "invoice_items",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(14,2)",
                oldPrecision: 14,
                oldScale: 2,
                oldNullable: true,
                oldComment: "Descuento aplicado al item (si aplica).");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_date",
                table: "invoice_items",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldComment: "Fecha de creación del item (UTC).");

            migrationBuilder.AlterColumn<Guid>(
                name: "created_by_id",
                table: "invoice_items",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldComment: "Usuario que creó el item.");

            migrationBuilder.AddColumn<Guid>(
                name: "package_id",
                table: "invoice_items",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "idx_invoice_number",
                table: "invoices",
                column: "number");

            migrationBuilder.CreateIndex(
                name: "idx_invoice_user",
                table: "invoices",
                column: "user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_invoice_items_health_services_service_id",
                table: "invoice_items",
                column: "service_id",
                principalTable: "health_services",
                principalColumn: "id");
        }
    }
}
