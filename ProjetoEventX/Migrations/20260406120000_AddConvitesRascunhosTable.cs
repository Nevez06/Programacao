using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ProjetoEventX.Migrations
{
    /// <inheritdoc />
    public partial class AddConvitesRascunhosTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConvitesRascunhos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EventoId = table.Column<int>(type: "integer", nullable: false),
                    TemplateId = table.Column<int>(type: "integer", nullable: true),
                    OrganizadorId = table.Column<int>(type: "integer", nullable: false),
                    NomeRascunho = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    LayoutJson = table.Column<string>(type: "text", nullable: false),
                    PreviewHtml = table.Column<string>(type: "text", nullable: true),
                    PreviewUrl = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConvitesRascunhos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConvitesRascunhos_Eventos_EventoId",
                        column: x => x.EventoId,
                        principalTable: "Eventos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConvitesRascunhos_TemplateConvites_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "TemplateConvites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConvitesRascunhos_EventoId_UpdatedAt",
                table: "ConvitesRascunhos",
                columns: new[] { "EventoId", "UpdatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ConvitesRascunhos_TemplateId",
                table: "ConvitesRascunhos",
                column: "TemplateId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConvitesRascunhos");
        }
    }
}
