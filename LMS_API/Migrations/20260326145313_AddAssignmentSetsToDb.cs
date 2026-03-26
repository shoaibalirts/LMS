using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS_API.Migrations
{
    /// <inheritdoc />
    public partial class AddAssignmentSetsToDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AssignmentSetId",
                table: "Assignments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AssignmentSets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TeacherId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssignmentSets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssignmentSets_Teacher_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Teacher",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Assignments",
                keyColumn: "Id",
                keyValue: 1,
                column: "AssignmentSetId",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_AssignmentSetId",
                table: "Assignments",
                column: "AssignmentSetId");

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentSets_TeacherId",
                table: "AssignmentSets",
                column: "TeacherId");

            migrationBuilder.AddForeignKey(
                name: "FK_Assignments_AssignmentSets_AssignmentSetId",
                table: "Assignments",
                column: "AssignmentSetId",
                principalTable: "AssignmentSets",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assignments_AssignmentSets_AssignmentSetId",
                table: "Assignments");

            migrationBuilder.DropTable(
                name: "AssignmentSets");

            migrationBuilder.DropIndex(
                name: "IX_Assignments_AssignmentSetId",
                table: "Assignments");

            migrationBuilder.DropColumn(
                name: "AssignmentSetId",
                table: "Assignments");
        }
    }
}
