using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjetoEventX.Migrations
{
    /// <inheritdoc />
    public partial class IntegracaoPedidosDespesas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "DespesaGerada",
                table: "Pedidos",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Origem",
                table: "Despesas",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PedidoId",
                table: "Despesas",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Despesas_PedidoId",
                table: "Despesas",
                column: "PedidoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Despesas_Pedidos_PedidoId",
                table: "Despesas",
                column: "PedidoId",
                principalTable: "Pedidos",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Despesas_Pedidos_PedidoId",
                table: "Despesas");

            migrationBuilder.DropIndex(
                name: "IX_Despesas_PedidoId",
                table: "Despesas");

            migrationBuilder.DropColumn(
                name: "DespesaGerada",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "Origem",
                table: "Despesas");

            migrationBuilder.DropColumn(
                name: "PedidoId",
                table: "Despesas");
        }
    }
}
