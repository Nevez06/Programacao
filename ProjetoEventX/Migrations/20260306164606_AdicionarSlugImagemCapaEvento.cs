using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjetoEventX.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarSlugImagemCapaEvento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImagemCapa",
                table: "Eventos",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Eventos",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Eventos_Slug",
                table: "Eventos",
                column: "Slug",
                unique: true,
                filter: "\"Slug\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Eventos_Slug",
                table: "Eventos");

            migrationBuilder.DropColumn(
                name: "ImagemCapa",
                table: "Eventos");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Eventos");
        }
    }
}
