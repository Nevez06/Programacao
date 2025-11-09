using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ProjetoEventX.Migrations.EventX
{
    /// <inheritdoc />
    public partial class InitialClean : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Administracoes_AspNetUsers_OrganizadorId",
                table: "Administracoes");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Pessoas_Fornecedor_PessoaId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Pessoas_Fornecedor_PessoaId1",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Pessoas_Organizador_PessoaId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Pessoas_Organizador_PessoaId1",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Pessoas_PessoaId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Pessoas_PessoaId1",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Eventos_AspNetUsers_OrganizadorId",
                table: "Eventos");

            migrationBuilder.DropForeignKey(
                name: "FK_Feedbacks_AspNetUsers_FornecedorId",
                table: "Feedbacks");

            migrationBuilder.DropForeignKey(
                name: "FK_ListasConvidados_AspNetUsers_ConvidadoId",
                table: "ListasConvidados");

            migrationBuilder.DropForeignKey(
                name: "FK_MensagemChats_Eventos_EventoId",
                table: "MensagemChats");

            migrationBuilder.DropForeignKey(
                name: "FK_MensagemChats_Pessoas_DestinatarioId",
                table: "MensagemChats");

            migrationBuilder.DropForeignKey(
                name: "FK_MensagemChats_Pessoas_RemetenteId",
                table: "MensagemChats");

            migrationBuilder.DropForeignKey(
                name: "FK_Pedidos_AspNetUsers_FornecedorId",
                table: "Pedidos");

            migrationBuilder.DropForeignKey(
                name: "FK_Pedidos_Eventos_EventoId",
                table: "Pedidos");

            migrationBuilder.DropForeignKey(
                name: "FK_Pedidos_Produto_ProdutoId",
                table: "Pedidos");

            migrationBuilder.DropForeignKey(
                name: "FK_Produto_AspNetUsers_FornecedorId",
                table: "Produto");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_Fornecedor_PessoaId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_Fornecedor_PessoaId1",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_Organizador_PessoaId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_Organizador_PessoaId1",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_PessoaId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_PessoaId1",
                table: "AspNetUsers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Produto",
                table: "Produto");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MensagemChats",
                table: "MensagemChats");

            migrationBuilder.DropColumn(
                name: "AvaliacaoMedia",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Cnpj",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ConfirmaPresenca",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "DataCadastro",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Fornecedor_CreatedAt",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Fornecedor_PessoaId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Fornecedor_PessoaId1",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Fornecedor_UpdatedAt",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Organizador_CreatedAt",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Organizador_DataCadastro",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Organizador_PessoaId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Organizador_PessoaId1",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Organizador_UpdatedAt",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "PessoaId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "PessoaId1",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "TipoServico",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "AspNetUsers");

            migrationBuilder.RenameTable(
                name: "Produto",
                newName: "Produtos");

            migrationBuilder.RenameTable(
                name: "MensagemChats",
                newName: "MensagemChat");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Pedidos",
                newName: "StatusPedido");

            migrationBuilder.RenameIndex(
                name: "IX_Produto_FornecedorId",
                table: "Produtos",
                newName: "IX_Produtos_FornecedorId");

            migrationBuilder.RenameIndex(
                name: "IX_MensagemChats_RemetenteId",
                table: "MensagemChat",
                newName: "IX_MensagemChat_RemetenteId");

            migrationBuilder.RenameIndex(
                name: "IX_MensagemChats_EventoId",
                table: "MensagemChat",
                newName: "IX_MensagemChat_EventoId");

            migrationBuilder.RenameIndex(
                name: "IX_MensagemChats_DestinatarioId",
                table: "MensagemChat",
                newName: "IX_MensagemChat_DestinatarioId");

            migrationBuilder.AddColumn<string>(
                name: "TipoUsuario",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Produtos",
                table: "Produtos",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MensagemChat",
                table: "MensagemChat",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Convidados",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PessoaId = table.Column<int>(type: "integer", nullable: false),
                    ConfirmaPresenca = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserName = table.Column<string>(type: "text", nullable: true),
                    NormalizedUserName = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    NormalizedEmail = table.Column<string>(type: "text", nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Convidados", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Convidados_Pessoas_PessoaId",
                        column: x => x.PessoaId,
                        principalTable: "Pessoas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Despesas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EventoId = table.Column<int>(type: "integer", nullable: false),
                    Descricao = table.Column<string>(type: "text", nullable: false),
                    Valor = table.Column<decimal>(type: "numeric", nullable: false),
                    DataDespesa = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Despesas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Despesas_Eventos_EventoId",
                        column: x => x.EventoId,
                        principalTable: "Eventos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Fornecedores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PessoaId = table.Column<int>(type: "integer", nullable: false),
                    Cnpj = table.Column<string>(type: "character varying(18)", maxLength: 18, nullable: false),
                    TipoServico = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    AvaliacaoMedia = table.Column<decimal>(type: "numeric", nullable: false),
                    DataCadastro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserName = table.Column<string>(type: "text", nullable: true),
                    NormalizedUserName = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    NormalizedEmail = table.Column<string>(type: "text", nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fornecedores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Fornecedores_Pessoas_PessoaId",
                        column: x => x.PessoaId,
                        principalTable: "Pessoas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Organizadores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PessoaId = table.Column<int>(type: "integer", nullable: false),
                    DataCadastro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserName = table.Column<string>(type: "text", nullable: true),
                    NormalizedUserName = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    NormalizedEmail = table.Column<string>(type: "text", nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organizadores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Organizadores_Pessoas_PessoaId",
                        column: x => x.PessoaId,
                        principalTable: "Pessoas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Convidados_PessoaId",
                table: "Convidados",
                column: "PessoaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Despesas_EventoId",
                table: "Despesas",
                column: "EventoId");

            migrationBuilder.CreateIndex(
                name: "IX_Fornecedores_PessoaId",
                table: "Fornecedores",
                column: "PessoaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Organizadores_PessoaId",
                table: "Organizadores",
                column: "PessoaId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Administracoes_Organizadores_OrganizadorId",
                table: "Administracoes",
                column: "OrganizadorId",
                principalTable: "Organizadores",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Eventos_Organizadores_OrganizadorId",
                table: "Eventos",
                column: "OrganizadorId",
                principalTable: "Organizadores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Feedbacks_Fornecedores_FornecedorId",
                table: "Feedbacks",
                column: "FornecedorId",
                principalTable: "Fornecedores",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ListasConvidados_Convidados_ConvidadoId",
                table: "ListasConvidados",
                column: "ConvidadoId",
                principalTable: "Convidados",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MensagemChat_Eventos_EventoId",
                table: "MensagemChat",
                column: "EventoId",
                principalTable: "Eventos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MensagemChat_Pessoas_DestinatarioId",
                table: "MensagemChat",
                column: "DestinatarioId",
                principalTable: "Pessoas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MensagemChat_Pessoas_RemetenteId",
                table: "MensagemChat",
                column: "RemetenteId",
                principalTable: "Pessoas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Pedidos_Eventos_EventoId",
                table: "Pedidos",
                column: "EventoId",
                principalTable: "Eventos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Pedidos_Fornecedores_FornecedorId",
                table: "Pedidos",
                column: "FornecedorId",
                principalTable: "Fornecedores",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Pedidos_Produtos_ProdutoId",
                table: "Pedidos",
                column: "ProdutoId",
                principalTable: "Produtos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Produtos_Fornecedores_FornecedorId",
                table: "Produtos",
                column: "FornecedorId",
                principalTable: "Fornecedores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Administracoes_Organizadores_OrganizadorId",
                table: "Administracoes");

            migrationBuilder.DropForeignKey(
                name: "FK_Eventos_Organizadores_OrganizadorId",
                table: "Eventos");

            migrationBuilder.DropForeignKey(
                name: "FK_Feedbacks_Fornecedores_FornecedorId",
                table: "Feedbacks");

            migrationBuilder.DropForeignKey(
                name: "FK_ListasConvidados_Convidados_ConvidadoId",
                table: "ListasConvidados");

            migrationBuilder.DropForeignKey(
                name: "FK_MensagemChat_Eventos_EventoId",
                table: "MensagemChat");

            migrationBuilder.DropForeignKey(
                name: "FK_MensagemChat_Pessoas_DestinatarioId",
                table: "MensagemChat");

            migrationBuilder.DropForeignKey(
                name: "FK_MensagemChat_Pessoas_RemetenteId",
                table: "MensagemChat");

            migrationBuilder.DropForeignKey(
                name: "FK_Pedidos_Eventos_EventoId",
                table: "Pedidos");

            migrationBuilder.DropForeignKey(
                name: "FK_Pedidos_Fornecedores_FornecedorId",
                table: "Pedidos");

            migrationBuilder.DropForeignKey(
                name: "FK_Pedidos_Produtos_ProdutoId",
                table: "Pedidos");

            migrationBuilder.DropForeignKey(
                name: "FK_Produtos_Fornecedores_FornecedorId",
                table: "Produtos");

            migrationBuilder.DropTable(
                name: "Convidados");

            migrationBuilder.DropTable(
                name: "Despesas");

            migrationBuilder.DropTable(
                name: "Fornecedores");

            migrationBuilder.DropTable(
                name: "Organizadores");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Produtos",
                table: "Produtos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MensagemChat",
                table: "MensagemChat");

            migrationBuilder.DropColumn(
                name: "TipoUsuario",
                table: "AspNetUsers");

            migrationBuilder.RenameTable(
                name: "Produtos",
                newName: "Produto");

            migrationBuilder.RenameTable(
                name: "MensagemChat",
                newName: "MensagemChats");

            migrationBuilder.RenameColumn(
                name: "StatusPedido",
                table: "Pedidos",
                newName: "Status");

            migrationBuilder.RenameIndex(
                name: "IX_Produtos_FornecedorId",
                table: "Produto",
                newName: "IX_Produto_FornecedorId");

            migrationBuilder.RenameIndex(
                name: "IX_MensagemChat_RemetenteId",
                table: "MensagemChats",
                newName: "IX_MensagemChats_RemetenteId");

            migrationBuilder.RenameIndex(
                name: "IX_MensagemChat_EventoId",
                table: "MensagemChats",
                newName: "IX_MensagemChats_EventoId");

            migrationBuilder.RenameIndex(
                name: "IX_MensagemChat_DestinatarioId",
                table: "MensagemChats",
                newName: "IX_MensagemChats_DestinatarioId");

            migrationBuilder.AddColumn<decimal>(
                name: "AvaliacaoMedia",
                table: "AspNetUsers",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Cnpj",
                table: "AspNetUsers",
                type: "character varying(18)",
                maxLength: 18,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConfirmaPresenca",
                table: "AspNetUsers",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataCadastro",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "AspNetUsers",
                type: "character varying(21)",
                maxLength: 21,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "Fornecedor_CreatedAt",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Fornecedor_PessoaId",
                table: "AspNetUsers",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Fornecedor_PessoaId1",
                table: "AspNetUsers",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Fornecedor_UpdatedAt",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Organizador_CreatedAt",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Organizador_DataCadastro",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Organizador_PessoaId",
                table: "AspNetUsers",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Organizador_PessoaId1",
                table: "AspNetUsers",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Organizador_UpdatedAt",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PessoaId",
                table: "AspNetUsers",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PessoaId1",
                table: "AspNetUsers",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipoServico",
                table: "AspNetUsers",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Produto",
                table: "Produto",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MensagemChats",
                table: "MensagemChats",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_Fornecedor_PessoaId",
                table: "AspNetUsers",
                column: "Fornecedor_PessoaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_Fornecedor_PessoaId1",
                table: "AspNetUsers",
                column: "Fornecedor_PessoaId1",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_Organizador_PessoaId",
                table: "AspNetUsers",
                column: "Organizador_PessoaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_Organizador_PessoaId1",
                table: "AspNetUsers",
                column: "Organizador_PessoaId1",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_PessoaId",
                table: "AspNetUsers",
                column: "PessoaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_PessoaId1",
                table: "AspNetUsers",
                column: "PessoaId1",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Administracoes_AspNetUsers_OrganizadorId",
                table: "Administracoes",
                column: "OrganizadorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Pessoas_Fornecedor_PessoaId",
                table: "AspNetUsers",
                column: "Fornecedor_PessoaId",
                principalTable: "Pessoas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Pessoas_Fornecedor_PessoaId1",
                table: "AspNetUsers",
                column: "Fornecedor_PessoaId1",
                principalTable: "Pessoas",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Pessoas_Organizador_PessoaId",
                table: "AspNetUsers",
                column: "Organizador_PessoaId",
                principalTable: "Pessoas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Pessoas_Organizador_PessoaId1",
                table: "AspNetUsers",
                column: "Organizador_PessoaId1",
                principalTable: "Pessoas",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Pessoas_PessoaId",
                table: "AspNetUsers",
                column: "PessoaId",
                principalTable: "Pessoas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Pessoas_PessoaId1",
                table: "AspNetUsers",
                column: "PessoaId1",
                principalTable: "Pessoas",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Eventos_AspNetUsers_OrganizadorId",
                table: "Eventos",
                column: "OrganizadorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Feedbacks_AspNetUsers_FornecedorId",
                table: "Feedbacks",
                column: "FornecedorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ListasConvidados_AspNetUsers_ConvidadoId",
                table: "ListasConvidados",
                column: "ConvidadoId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MensagemChats_Eventos_EventoId",
                table: "MensagemChats",
                column: "EventoId",
                principalTable: "Eventos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MensagemChats_Pessoas_DestinatarioId",
                table: "MensagemChats",
                column: "DestinatarioId",
                principalTable: "Pessoas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MensagemChats_Pessoas_RemetenteId",
                table: "MensagemChats",
                column: "RemetenteId",
                principalTable: "Pessoas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Pedidos_AspNetUsers_FornecedorId",
                table: "Pedidos",
                column: "FornecedorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Pedidos_Eventos_EventoId",
                table: "Pedidos",
                column: "EventoId",
                principalTable: "Eventos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Pedidos_Produto_ProdutoId",
                table: "Pedidos",
                column: "ProdutoId",
                principalTable: "Produto",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Produto_AspNetUsers_FornecedorId",
                table: "Produto",
                column: "FornecedorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
