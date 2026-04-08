using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ProjetoEventX.Migrations
{
    /// <inheritdoc />
    public partial class AddEventXSocialStoriesAndSavedPosts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PerfisSociais",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    NomeExibicao = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Bio = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FotoPerfilUrl = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    TipoPerfil = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Cidade = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    Instagram = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    Site = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerfisSociais", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PerfisSociais_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SocialPosts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    PerfilSocialId = table.Column<int>(type: "integer", nullable: false),
                    EventoId = table.Column<int>(type: "integer", nullable: true),
                    Titulo = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    Legenda = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    ImagemUrl = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Categoria = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    TipoConteudo = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    Localizacao = table.Column<string>(type: "character varying(140)", maxLength: 140, nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SocialPosts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SocialPosts_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SocialPosts_Eventos_EventoId",
                        column: x => x.EventoId,
                        principalTable: "Eventos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SocialPosts_PerfisSociais_PerfilSocialId",
                        column: x => x.PerfilSocialId,
                        principalTable: "PerfisSociais",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SocialStatus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    PerfilSocialId = table.Column<int>(type: "integer", nullable: false),
                    ImagemUrl = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    TextoOverlay = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiraEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SocialStatus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SocialStatus_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SocialStatus_PerfisSociais_PerfilSocialId",
                        column: x => x.PerfilSocialId,
                        principalTable: "PerfisSociais",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SocialComentarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PostId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    PerfilSocialId = table.Column<int>(type: "integer", nullable: true),
                    Texto = table.Column<string>(type: "character varying(600)", maxLength: 600, nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SocialComentarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SocialComentarios_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SocialComentarios_PerfisSociais_PerfilSocialId",
                        column: x => x.PerfilSocialId,
                        principalTable: "PerfisSociais",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SocialComentarios_SocialPosts_PostId",
                        column: x => x.PostId,
                        principalTable: "SocialPosts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SocialCurtidas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PostId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SocialCurtidas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SocialCurtidas_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SocialCurtidas_SocialPosts_PostId",
                        column: x => x.PostId,
                        principalTable: "SocialPosts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SocialPostsSalvos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PostId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SocialPostsSalvos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SocialPostsSalvos_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SocialPostsSalvos_SocialPosts_PostId",
                        column: x => x.PostId,
                        principalTable: "SocialPosts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SocialStatusVisualizacoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SocialStatusId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    VisualizadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SocialStatusVisualizacoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SocialStatusVisualizacoes_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SocialStatusVisualizacoes_SocialStatus_SocialStatusId",
                        column: x => x.SocialStatusId,
                        principalTable: "SocialStatus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PerfisSociais_UserId",
                table: "PerfisSociais",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SocialComentarios_PerfilSocialId",
                table: "SocialComentarios",
                column: "PerfilSocialId");

            migrationBuilder.CreateIndex(
                name: "IX_SocialComentarios_PostId",
                table: "SocialComentarios",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "IX_SocialComentarios_UserId",
                table: "SocialComentarios",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SocialCurtidas_PostId_UserId",
                table: "SocialCurtidas",
                columns: new[] { "PostId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SocialCurtidas_UserId",
                table: "SocialCurtidas",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SocialPosts_EventoId",
                table: "SocialPosts",
                column: "EventoId");

            migrationBuilder.CreateIndex(
                name: "IX_SocialPosts_PerfilSocialId",
                table: "SocialPosts",
                column: "PerfilSocialId");

            migrationBuilder.CreateIndex(
                name: "IX_SocialPosts_UserId",
                table: "SocialPosts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SocialPostsSalvos_PostId_UserId",
                table: "SocialPostsSalvos",
                columns: new[] { "PostId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SocialPostsSalvos_UserId",
                table: "SocialPostsSalvos",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SocialStatus_PerfilSocialId_ExpiraEm_Ativo",
                table: "SocialStatus",
                columns: new[] { "PerfilSocialId", "ExpiraEm", "Ativo" });

            migrationBuilder.CreateIndex(
                name: "IX_SocialStatus_UserId",
                table: "SocialStatus",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SocialStatusVisualizacoes_SocialStatusId_UserId",
                table: "SocialStatusVisualizacoes",
                columns: new[] { "SocialStatusId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SocialStatusVisualizacoes_UserId",
                table: "SocialStatusVisualizacoes",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SocialComentarios");

            migrationBuilder.DropTable(
                name: "SocialCurtidas");

            migrationBuilder.DropTable(
                name: "SocialPostsSalvos");

            migrationBuilder.DropTable(
                name: "SocialStatusVisualizacoes");

            migrationBuilder.DropTable(
                name: "SocialPosts");

            migrationBuilder.DropTable(
                name: "SocialStatus");

            migrationBuilder.DropTable(
                name: "PerfisSociais");
        }
    }
}
