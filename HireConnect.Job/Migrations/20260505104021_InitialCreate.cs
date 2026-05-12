using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HireConnect.Job.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Jobs",
                columns: table => new
                {
                    JobId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Location = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    SalaryMin = table.Column<double>(type: "double precision", nullable: false),
                    SalaryMax = table.Column<double>(type: "double precision", nullable: false),
                    Skills = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false, defaultValue: ""),
                    ExperienceRequired = table.Column<int>(type: "integer", nullable: false),
                    PostedBy = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Active"),
                    PostedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    Description = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jobs", x => x.JobId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_Category",
                table: "Jobs",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_Location",
                table: "Jobs",
                column: "Location");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_PostedAt",
                table: "Jobs",
                column: "PostedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_PostedBy",
                table: "Jobs",
                column: "PostedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_Status",
                table: "Jobs",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Jobs");
        }
    }
}
