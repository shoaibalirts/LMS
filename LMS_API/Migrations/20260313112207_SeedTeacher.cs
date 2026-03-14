using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS_API.Migrations
{
    /// <inheritdoc />
    public partial class SeedTeacher : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "Teacher",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "Teacher",
                type: "datetime2",
                nullable: true);

            migrationBuilder.InsertData(
                table: "Teacher",
                columns: new[] { "Id", "CreatedDate", "Email", "FirstName", "LastName", "Password", "UpdatedDate" },
                values: new object[] { 1, new DateTime(2026, 3, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), "morten.domsgard@ucl.dk", "Morten", "Domsgard", "1234567890", new DateTime(2026, 3, 13, 0, 0, 0, 0, DateTimeKind.Unspecified) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Teacher",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "Teacher");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "Teacher");
        }
    }
}
