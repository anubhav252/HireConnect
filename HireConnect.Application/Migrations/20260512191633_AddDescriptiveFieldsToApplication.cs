using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HireConnect.Application.Migrations
{
    /// <inheritdoc />
    public partial class AddDescriptiveFieldsToApplication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CandidateEmail",
                table: "Applications",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "JobTitle",
                table: "Applications",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CandidateEmail",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "JobTitle",
                table: "Applications");
        }
    }
}
