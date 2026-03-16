using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIGREF.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateReportHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_report_history_user",
                table: "report_history");

            migrationBuilder.DropColumn(
                name: "created_by_user_id",
                table: "report_history");

            migrationBuilder.DropColumn(
                name: "display",
                table: "report_history");

            migrationBuilder.DropColumn(
                name: "format",
                table: "report_history");

            migrationBuilder.DropColumn(
                name: "system_display",
                table: "report_history");

            migrationBuilder.RenameColumn(
                name: "hospital_properties_snapshot",
                table: "report_history",
                newName: "HospitalPropertiesSnapshot");

            migrationBuilder.RenameColumn(
                name: "logo_media_id",
                table: "hospital_properties",
                newName: "LogoMediaId");

            migrationBuilder.RenameColumn(
                name: "health_logo_media_id",
                table: "hospital_properties",
                newName: "HealthLogoMediaId");

            migrationBuilder.AlterTable(
                name: "invoice_serie",
                comment: "Tabla para gestionar las series de facturación y el control de su correlativo actual.");

            /*migrationBuilder.AlterColumn<string>(
                name: "HospitalPropertiesSnapshot",
                table: "report_history",
                type: "jsonb",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text"); */
            migrationBuilder.Sql(
                @"ALTER TABLE report_history ALTER COLUMN ""HospitalPropertiesSnapshot"" TYPE jsonb USING ""HospitalPropertiesSnapshot""::jsonb;");

            migrationBuilder.AddColumn<string>(
                name: "DownloadUrl",
                table: "report_history",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ErrorMessage",
                table: "report_history",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HangfireJobId",
                table: "report_history",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PeriodLabel",
                table: "report_history",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Progress",
                table: "report_history",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "RequestedByUserId",
                table: "report_history",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "report_history",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "filter_json",
                table: "report_history",
                type: "jsonb",
                nullable: true,
                comment: "JSON Filter del reporte del hospital.");

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_date",
                table: "invoice_serie",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Fecha de última actualización (UTC).",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "updated_by_id",
                table: "invoice_serie",
                type: "uuid",
                nullable: true,
                comment: "ID del usuario que realizó la última actualización.",
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "start_number",
                table: "invoice_serie",
                type: "bigint",
                nullable: false,
                comment: "Número inicial autorizado para esta serie",
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<string>(
                name: "prefix",
                table: "invoice_serie",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                comment: "Prefijo de la factura (ej: F001, A)",
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "invoice_serie",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                comment: "Nombre descriptivo de la serie (ej: Serie A - Principal)",
                oldClrType: typeof(string),
                oldType: "character varying(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<long>(
                name: "end_number",
                table: "invoice_serie",
                type: "bigint",
                nullable: false,
                comment: "Número final autorizado para esta serie",
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<long>(
                name: "current_number",
                table: "invoice_serie",
                type: "bigint",
                nullable: false,
                comment: "Último número de factura emitido en esta serie",
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_date",
                table: "invoice_serie",
                type: "timestamp with time zone",
                nullable: false,
                comment: "Fecha de creación del turno (UTC).",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<Guid>(
                name: "created_by_id",
                table: "invoice_serie",
                type: "uuid",
                nullable: false,
                comment: "ID del usuario que creó el registro.",
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "currency",
                table: "hospital_properties",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "idx_report_history_user",
                table: "report_history",
                column: "created_by_id");

            migrationBuilder.CreateIndex(
                name: "idx_invoice_serie_name",
                table: "invoice_serie",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "idx_invoice_serie_prefix",
                table: "invoice_serie",
                column: "prefix",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_report_history_user",
                table: "report_history");

            migrationBuilder.DropIndex(
                name: "idx_invoice_serie_name",
                table: "invoice_serie");

            migrationBuilder.DropIndex(
                name: "idx_invoice_serie_prefix",
                table: "invoice_serie");

            migrationBuilder.DropColumn(
                name: "DownloadUrl",
                table: "report_history");

            migrationBuilder.DropColumn(
                name: "ErrorMessage",
                table: "report_history");

            migrationBuilder.DropColumn(
                name: "HangfireJobId",
                table: "report_history");

            migrationBuilder.DropColumn(
                name: "PeriodLabel",
                table: "report_history");

            migrationBuilder.DropColumn(
                name: "Progress",
                table: "report_history");

            migrationBuilder.DropColumn(
                name: "RequestedByUserId",
                table: "report_history");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "report_history");

            migrationBuilder.DropColumn(
                name: "filter_json",
                table: "report_history");

            migrationBuilder.RenameColumn(
                name: "HospitalPropertiesSnapshot",
                table: "report_history",
                newName: "hospital_properties_snapshot");

            migrationBuilder.RenameColumn(
                name: "LogoMediaId",
                table: "hospital_properties",
                newName: "logo_media_id");

            migrationBuilder.RenameColumn(
                name: "HealthLogoMediaId",
                table: "hospital_properties",
                newName: "health_logo_media_id");

            migrationBuilder.AlterTable(
                name: "invoice_serie",
                oldComment: "Tabla para gestionar las series de facturación y el control de su correlativo actual.");

           /* migrationBuilder.AlterColumn<string>(
                name: "hospital_properties_snapshot",
                table: "report_history",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "jsonb"); */
            migrationBuilder.Sql(@"ALTER TABLE report_history ALTER COLUMN ""HospitalPropertiesSnapshot"" TYPE text USING ""HospitalPropertiesSnapshot""::text;");

            migrationBuilder.AddColumn<Guid>(
                name: "created_by_user_id",
                table: "report_history",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "display",
                table: "report_history",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "format",
                table: "report_history",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "system_display",
                table: "report_history",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_date",
                table: "invoice_serie",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldComment: "Fecha de última actualización (UTC).");

            migrationBuilder.AlterColumn<Guid>(
                name: "updated_by_id",
                table: "invoice_serie",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true,
                oldComment: "ID del usuario que realizó la última actualización.");

            migrationBuilder.AlterColumn<long>(
                name: "start_number",
                table: "invoice_serie",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldComment: "Número inicial autorizado para esta serie");

            migrationBuilder.AlterColumn<string>(
                name: "prefix",
                table: "invoice_serie",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10,
                oldComment: "Prefijo de la factura (ej: F001, A)");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "invoice_serie",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(150)",
                oldMaxLength: 150,
                oldComment: "Nombre descriptivo de la serie (ej: Serie A - Principal)");

            migrationBuilder.AlterColumn<long>(
                name: "end_number",
                table: "invoice_serie",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldComment: "Número final autorizado para esta serie");

            migrationBuilder.AlterColumn<long>(
                name: "current_number",
                table: "invoice_serie",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldComment: "Último número de factura emitido en esta serie");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_date",
                table: "invoice_serie",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldComment: "Fecha de creación del turno (UTC).");

            migrationBuilder.AlterColumn<Guid>(
                name: "created_by_id",
                table: "invoice_serie",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldComment: "ID del usuario que creó el registro.");

            migrationBuilder.AlterColumn<string>(
                name: "currency",
                table: "hospital_properties",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10);

            migrationBuilder.CreateIndex(
                name: "idx_report_history_user",
                table: "report_history",
                column: "created_by_user_id");
        }
    }
}
