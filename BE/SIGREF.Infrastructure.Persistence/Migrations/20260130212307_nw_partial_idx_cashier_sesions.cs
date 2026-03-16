using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIGREF.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class nw_partial_idx_cashier_sesions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // NO borrar idx_cashier_sessions_user (performance/histórico)

            // Elimina el índice viejo (antes era unique (user_id,is_open))
            migrationBuilder.DropIndex(
                name: "uq_cashier_sessions_user_open",
                table: "cashier_sessions");

            // Crea el índice único parcial correcto: solo 1 sesión abierta por usuario
            migrationBuilder.CreateIndex(
                name: "uq_cashier_sessions_user_open",
                table: "cashier_sessions",
                column: "user_id",
                unique: true,
                filter: "is_open = true");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revierte a como estaba antes (si haces rollback)

            migrationBuilder.DropIndex(
                name: "uq_cashier_sessions_user_open",
                table: "cashier_sessions");

            migrationBuilder.CreateIndex(
                name: "uq_cashier_sessions_user_open",
                table: "cashier_sessions",
                columns: new[] { "user_id", "is_open" },
                unique: true);

            // NO tocar idx_cashier_sessions_user aquí tampoco
        }
    }
}
