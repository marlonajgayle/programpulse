using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProgramPulse.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddKpiPlanningFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Activities",
                table: "Kpis",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KeyOutputs",
                table: "Kpis",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PerformanceMeasure",
                table: "Kpis",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Strategies",
                table: "Kpis",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Activities",
                table: "Kpis");

            migrationBuilder.DropColumn(
                name: "KeyOutputs",
                table: "Kpis");

            migrationBuilder.DropColumn(
                name: "PerformanceMeasure",
                table: "Kpis");

            migrationBuilder.DropColumn(
                name: "Strategies",
                table: "Kpis");
        }
    }
}
