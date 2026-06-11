using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIGREF.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddHealthLogoMediaId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "health_logo_media_id",
                table: "hospital_properties",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "health_logo_media_id",
                table: "hospital_properties");
        }
    }
}
