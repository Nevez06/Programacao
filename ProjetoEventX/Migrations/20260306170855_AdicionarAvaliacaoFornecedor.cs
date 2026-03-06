using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ProjetoEventX.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarAvaliacaoFornecedor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AvaliacoesFornecedores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nota = table.Column<int>(type: "integer", nullable: false),
                    Comentario = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    FornecedorId = table.Column<int>(type: "integer", nullable: false),
                    OrganizadorId = table.Column<int>(type: "integer", nullable: false),
                    EventoId = table.Column<int>(type: "integer", nullable: false),
                    DataAvaliacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AvaliacoesFornecedores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AvaliacoesFornecedores_Eventos_EventoId",
                        column: x => x.EventoId,
                        principalTable: "Eventos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AvaliacoesFornecedores_Fornecedores_FornecedorId",
                        column: x => x.FornecedorId,
                        principalTable: "Fornecedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AvaliacoesFornecedores_Organizadores_OrganizadorId",
                        column: x => x.OrganizadorId,
                        principalTable: "Organizadores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AvaliacoesFornecedores_EventoId",
                table: "AvaliacoesFornecedores",
                column: "EventoId");

            migrationBuilder.CreateIndex(
                name: "IX_AvaliacoesFornecedores_FornecedorId_OrganizadorId_EventoId",
                table: "AvaliacoesFornecedores",
                columns: new[] { "FornecedorId", "OrganizadorId", "EventoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AvaliacoesFornecedores_OrganizadorId",
                table: "AvaliacoesFornecedores",
                column: "OrganizadorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AvaliacoesFornecedores");
        }
    }
}
