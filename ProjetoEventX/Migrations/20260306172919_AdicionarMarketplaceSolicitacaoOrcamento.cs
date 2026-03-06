using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ProjetoEventX.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarMarketplaceSolicitacaoOrcamento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SolicitacoesOrcamento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrganizadorId = table.Column<int>(type: "integer", nullable: false),
                    FornecedorId = table.Column<int>(type: "integer", nullable: false),
                    EventoId = table.Column<int>(type: "integer", nullable: true),
                    Titulo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    TipoServicoDesejado = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DataEvento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LocalEvento = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    OrcamentoEstimado = table.Column<decimal>(type: "numeric", nullable: true),
                    QuantidadeConvidados = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "Pendente"),
                    RespostaFornecedor = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ValorProposto = table.Column<decimal>(type: "numeric", nullable: true),
                    DataSolicitacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataResposta = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitacoesOrcamento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolicitacoesOrcamento_Eventos_EventoId",
                        column: x => x.EventoId,
                        principalTable: "Eventos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SolicitacoesOrcamento_Fornecedores_FornecedorId",
                        column: x => x.FornecedorId,
                        principalTable: "Fornecedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SolicitacoesOrcamento_Organizadores_OrganizadorId",
                        column: x => x.OrganizadorId,
                        principalTable: "Organizadores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SolicitacoesOrcamento_EventoId",
                table: "SolicitacoesOrcamento",
                column: "EventoId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitacoesOrcamento_FornecedorId",
                table: "SolicitacoesOrcamento",
                column: "FornecedorId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitacoesOrcamento_OrganizadorId",
                table: "SolicitacoesOrcamento",
                column: "OrganizadorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SolicitacoesOrcamento");
        }
    }
}
