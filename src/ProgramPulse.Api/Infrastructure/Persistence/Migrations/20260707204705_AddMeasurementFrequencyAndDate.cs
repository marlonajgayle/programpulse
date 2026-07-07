using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProgramPulse.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMeasurementFrequencyAndDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "MeasurementDate",
                table: "Measurements",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            // Backfill existing measurements: their reading date is the row's creation date.
            migrationBuilder.Sql("UPDATE [Measurements] SET [MeasurementDate] = [CreatedDate];");

            migrationBuilder.AddColumn<string>(
                name: "MeasurementFrequency",
                table: "Kpis",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MeasurementDate",
                table: "Measurements");

            migrationBuilder.DropColumn(
                name: "MeasurementFrequency",
                table: "Kpis");
        }
    }
}
