using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS_API.Migrations
{
    /// <inheritdoc />
    public partial class StoreAssignedAssignmentResultPath : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StudentResult",
                table: "AssignedAssignments");

            migrationBuilder.AddColumn<string>(
                name: "StudentResultPath",
                table: "AssignedAssignments",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StudentResultPath",
                table: "AssignedAssignments");

            migrationBuilder.AddColumn<byte[]>(
                name: "StudentResult",
                table: "AssignedAssignments",
                type: "varbinary(max)",
                nullable: true);
        }
    }
}
