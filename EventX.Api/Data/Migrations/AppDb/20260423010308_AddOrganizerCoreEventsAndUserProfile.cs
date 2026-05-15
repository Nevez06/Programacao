using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EventX.Api.Data.Migrations.AppDb
{
    /// <inheritdoc />
    public partial class AddOrganizerCoreEventsAndUserProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "events",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Type = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    Description = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: true),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Location = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    CoverImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    EstimatedAudience = table.Column<int>(type: "integer", nullable: true),
                    EstimatedCost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    OrganizerId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_events_users_OrganizerId",
                        column: x => x.OrganizerId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_events_OrganizerId",
                table: "events",
                column: "OrganizerId");

            migrationBuilder.CreateIndex(
                name: "IX_events_StartDate",
                table: "events",
                column: "StartDate");

            migrationBuilder.CreateIndex(
                name: "IX_events_Status",
                table: "events",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "events");
        }
    }
}
