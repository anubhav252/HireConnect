using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HireConnect.Interview.Migrations
{
    /// <inheritdoc />
    public partial class AddCandidateIdToInterview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CandidateId",
                table: "Interviews",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CandidateId",
                table: "Interviews");
        }
    }
}
