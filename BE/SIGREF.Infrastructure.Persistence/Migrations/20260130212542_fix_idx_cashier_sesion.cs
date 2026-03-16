using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIGREF.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class fix_idx_cashier_sesion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS idx_cashier_sessions_user
                ON cashier_sessions (user_id);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS idx_cashier_sessions_user;");
        }
    }
}
