using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ProjetoEventX.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarTabelaQuotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "CSSPersonalizado",
                table: "TemplatesConvites",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "Quotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EventId = table.Column<int>(type: "integer", nullable: false),
                    SupplierId = table.Column<int>(type: "integer", nullable: false),
                    OrganizadorId = table.Column<int>(type: "integer", nullable: false),
                    ServiceName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    EstimatedValue = table.Column<decimal>(type: "numeric", nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "Pendente"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ResponseMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ResponseValue = table.Column<decimal>(type: "numeric", nullable: true),
                    ResponseDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PedidoGeradoId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Quotes_Eventos_EventId",
                        column: x => x.EventId,
                        principalTable: "Eventos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Quotes_Fornecedores_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Fornecedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Quotes_Organizadores_OrganizadorId",
                        column: x => x.OrganizadorId,
                        principalTable: "Organizadores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Quotes_Pedidos_PedidoGeradoId",
                        column: x => x.PedidoGeradoId,
                        principalTable: "Pedidos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Quotes_EventId",
                table: "Quotes",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_Quotes_OrganizadorId",
                table: "Quotes",
                column: "OrganizadorId");

            migrationBuilder.CreateIndex(
                name: "IX_Quotes_PedidoGeradoId",
                table: "Quotes",
                column: "PedidoGeradoId");

            migrationBuilder.CreateIndex(
                name: "IX_Quotes_SupplierId",
                table: "Quotes",
                column: "SupplierId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Quotes");

            migrationBuilder.AlterColumn<string>(
                name: "CSSPersonalizado",
                table: "TemplatesConvites",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
