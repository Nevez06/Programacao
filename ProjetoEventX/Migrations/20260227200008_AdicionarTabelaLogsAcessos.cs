using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ProjetoEventX.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarTabelaLogsAcessos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Auditorias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TipoEntidade = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EntidadeId = table.Column<int>(type: "integer", nullable: false),
                    TipoAcao = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Usuario = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    DataAcao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EnderecoIP = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    Descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DadosAntigos = table.Column<string>(type: "TEXT", nullable: true),
                    DadosNovos = table.Column<string>(type: "TEXT", nullable: true),
                    Navegador = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SistemaOperacional = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Sucesso = table.Column<bool>(type: "boolean", nullable: true),
                    MensagemErro = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Auditorias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LogsAcessos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EnderecoIP = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    DataAcesso = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Usuario = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    UrlAcesso = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    UserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    TipoAcesso = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    AcessoBloqueado = table.Column<bool>(type: "boolean", nullable: true),
                    MotivoBloqueio = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogsAcessos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TemplatesConvites",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NomeTemplate = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EventoId = table.Column<int>(type: "integer", nullable: false),
                    OrganizadorId = table.Column<int>(type: "integer", nullable: false),
                    TituloConvite = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    MensagemPrincipal = table.Column<string>(type: "text", nullable: false),
                    MensagemSecundaria = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CorFundo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CorTexto = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CorPrimaria = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FonteTitulo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FonteTexto = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TamanhoFonteTitulo = table.Column<int>(type: "integer", nullable: false),
                    TamanhoFonteTexto = table.Column<int>(type: "integer", nullable: false),
                    MostrarLogo = table.Column<bool>(type: "boolean", nullable: false),
                    MostrarFotoEvento = table.Column<bool>(type: "boolean", nullable: false),
                    MostrarMapa = table.Column<bool>(type: "boolean", nullable: false),
                    MostrarQRCode = table.Column<bool>(type: "boolean", nullable: false),
                    ImagemCabecalho = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ImagemRodape = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    EstiloLayout = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CSSPersonalizado = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    PadraoSistema = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemplatesConvites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TemplatesConvites_Eventos_EventoId",
                        column: x => x.EventoId,
                        principalTable: "Eventos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TemplatesConvites_Organizadores_OrganizadorId",
                        column: x => x.OrganizadorId,
                        principalTable: "Organizadores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TemplatesConvites_EventoId",
                table: "TemplatesConvites",
                column: "EventoId");

            migrationBuilder.CreateIndex(
                name: "IX_TemplatesConvites_OrganizadorId",
                table: "TemplatesConvites",
                column: "OrganizadorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Auditorias");

            migrationBuilder.DropTable(
                name: "LogsAcessos");

            migrationBuilder.DropTable(
                name: "TemplatesConvites");
        }
    }
}
