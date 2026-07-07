using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProgramPulse.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ObjectiveKpiOneToOne : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Kpis_ObjectiveId",
                table: "Kpis");

            migrationBuilder.CreateIndex(
                name: "IX_Kpis_ObjectiveId",
                table: "Kpis",
                column: "ObjectiveId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Kpis_ObjectiveId",
                table: "Kpis");

            migrationBuilder.CreateIndex(
                name: "IX_Kpis_ObjectiveId",
                table: "Kpis",
                column: "ObjectiveId");
        }
    }
}
