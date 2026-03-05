using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjetoEventX.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarCheckInQRCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CheckInRealizado",
                table: "ListasConvidados",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "CodigoQR",
                table: "ListasConvidados",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataCheckIn",
                table: "ListasConvidados",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CheckInRealizado",
                table: "ListasConvidados");

            migrationBuilder.DropColumn(
                name: "CodigoQR",
                table: "ListasConvidados");

            migrationBuilder.DropColumn(
                name: "DataCheckIn",
                table: "ListasConvidados");
        }
    }
}
