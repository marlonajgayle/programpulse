using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProgramPulse.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddKpiCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Kpis",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Operational");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "Kpis");
        }
    }
}
