using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EventX.Api.Data.Migrations.AppDb
{
    /// <inheritdoc />
    public partial class AddQuotesModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "quotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EventId = table.Column<int>(type: "integer", nullable: false),
                    SupplierId = table.Column<int>(type: "integer", nullable: false),
                    OrganizerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceName = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    Description = table.Column<string>(type: "character varying(2500)", maxLength: 2500, nullable: false),
                    EstimatedValue = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ResponseMessage = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ResponseValue = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    ResponseDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CounterProposalValue = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    CounterProposalMessage = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CounterProposalDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CurrentRound = table.Column<int>(type: "integer", nullable: false),
                    ExpireAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    GeneratedOrderId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_quotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_quotes_events_EventId",
                        column: x => x.EventId,
                        principalTable: "events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_quotes_suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_quotes_users_OrganizerId",
                        column: x => x.OrganizerId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_quotes_CreatedAt",
                table: "quotes",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_quotes_EventId",
                table: "quotes",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_quotes_OrganizerId",
                table: "quotes",
                column: "OrganizerId");

            migrationBuilder.CreateIndex(
                name: "IX_quotes_Status",
                table: "quotes",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_quotes_SupplierId",
                table: "quotes",
                column: "SupplierId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "quotes");
        }
    }
}
