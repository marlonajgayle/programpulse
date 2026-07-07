using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProgramPulse.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameInitiativeToProgrammeAndAddParentStatus : Migration
    {
        // Hand-edited: the scaffolded version dropped and recreated the Initiatives
        // table (data loss). This version renames the table, columns, indexes and
        // constraints in place via sp_rename so existing rows are preserved, then
        // makes StartDate optional and adds the self-referencing parent link.
        // Status is derived from EndDate at read time and is not a stored column.

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Rename the table (sp_rename) — preserves all rows, PK data and FKs.
            migrationBuilder.RenameTable(
                name: "Initiatives",
                newName: "Programmes");

            // Rename the child FK column + its index on Objectives.
            migrationBuilder.RenameColumn(
                name: "InitiativeId",
                table: "Objectives",
                newName: "ProgrammeId");

            migrationBuilder.RenameIndex(
                name: "IX_Objectives_InitiativeId",
                table: "Objectives",
                newName: "IX_Objectives_ProgrammeId");

            // Rename the tenant index on the (now) Programmes table.
            migrationBuilder.RenameIndex(
                name: "IX_Initiatives_TenantId",
                table: "Programmes",
                newName: "IX_Programmes_TenantId");

            // Rename the PK and FK constraints to match the new table name.
            migrationBuilder.Sql("EXEC sp_rename N'PK_Initiatives', N'PK_Programmes';");
            migrationBuilder.Sql("EXEC sp_rename N'FK_Initiatives_Tenants_TenantId', N'FK_Programmes_Tenants_TenantId';");
            migrationBuilder.Sql("EXEC sp_rename N'FK_Objectives_Initiatives_InitiativeId', N'FK_Objectives_Programmes_ProgrammeId';");

            // StartDate is now optional.
            migrationBuilder.AlterColumn<DateTime>(
                name: "StartDate",
                table: "Programmes",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            // New optional self-reference to a parent programme.
            migrationBuilder.AddColumn<Guid>(
                name: "ParentProgrammeId",
                table: "Programmes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Programmes_ParentProgrammeId",
                table: "Programmes",
                column: "ParentProgrammeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Programmes_Programmes_ParentProgrammeId",
                table: "Programmes",
                column: "ParentProgrammeId",
                principalTable: "Programmes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Programmes_Programmes_ParentProgrammeId",
                table: "Programmes");

            migrationBuilder.DropIndex(
                name: "IX_Programmes_ParentProgrammeId",
                table: "Programmes");

            migrationBuilder.DropColumn(
                name: "ParentProgrammeId",
                table: "Programmes");

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartDate",
                table: "Programmes",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            // Reverse the constraint, index, column and table renames.
            migrationBuilder.Sql("EXEC sp_rename N'FK_Objectives_Programmes_ProgrammeId', N'FK_Objectives_Initiatives_InitiativeId';");
            migrationBuilder.Sql("EXEC sp_rename N'FK_Programmes_Tenants_TenantId', N'FK_Initiatives_Tenants_TenantId';");
            migrationBuilder.Sql("EXEC sp_rename N'PK_Programmes', N'PK_Initiatives';");

            migrationBuilder.RenameIndex(
                name: "IX_Programmes_TenantId",
                table: "Programmes",
                newName: "IX_Initiatives_TenantId");

            migrationBuilder.RenameIndex(
                name: "IX_Objectives_ProgrammeId",
                table: "Objectives",
                newName: "IX_Objectives_InitiativeId");

            migrationBuilder.RenameColumn(
                name: "ProgrammeId",
                table: "Objectives",
                newName: "InitiativeId");

            migrationBuilder.RenameTable(
                name: "Programmes",
                newName: "Initiatives");
        }
    }
}
