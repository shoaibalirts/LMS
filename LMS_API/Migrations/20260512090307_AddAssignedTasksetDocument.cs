using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS_API.Migrations
{
    /// <inheritdoc />
    public partial class AddAssignedTasksetDocument : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AssignmentSetId",
                table: "AssignedAssignmentSets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TaskDocumentContentType",
                table: "AssignedAssignmentSets",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TaskDocumentFileName",
                table: "AssignedAssignmentSets",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TaskDocumentPath",
                table: "AssignedAssignmentSets",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssignedAssignmentSets_AssignmentSetId",
                table: "AssignedAssignmentSets",
                column: "AssignmentSetId");

            migrationBuilder.AddForeignKey(
                name: "FK_AssignedAssignmentSets_AssignmentSets_AssignmentSetId",
                table: "AssignedAssignmentSets",
                column: "AssignmentSetId",
                principalTable: "AssignmentSets",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssignedAssignmentSets_AssignmentSets_AssignmentSetId",
                table: "AssignedAssignmentSets");

            migrationBuilder.DropIndex(
                name: "IX_AssignedAssignmentSets_AssignmentSetId",
                table: "AssignedAssignmentSets");

            migrationBuilder.DropColumn(
                name: "AssignmentSetId",
                table: "AssignedAssignmentSets");

            migrationBuilder.DropColumn(
                name: "TaskDocumentContentType",
                table: "AssignedAssignmentSets");

            migrationBuilder.DropColumn(
                name: "TaskDocumentFileName",
                table: "AssignedAssignmentSets");

            migrationBuilder.DropColumn(
                name: "TaskDocumentPath",
                table: "AssignedAssignmentSets");
        }
    }
}
