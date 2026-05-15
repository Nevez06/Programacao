using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EventX.Api.Data.Migrations.AppDb
{
    /// <inheritdoc />
    public partial class AddInvitationsTemplatesAndDrafts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "invitation_templates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Style = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    BackgroundColor = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    PrimaryColor = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    TextColor = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Font = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Message = table.Column<string>(type: "character varying(1200)", maxLength: 1200, nullable: true),
                    PreviewUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DefaultSystem = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    OrganizerId = table.Column<Guid>(type: "uuid", nullable: true),
                    EventId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_invitation_templates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_invitation_templates_events_EventId",
                        column: x => x.EventId,
                        principalTable: "events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_invitation_templates_users_OrganizerId",
                        column: x => x.OrganizerId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "invitation_drafts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EventId = table.Column<int>(type: "integer", nullable: false),
                    OrganizerId = table.Column<Guid>(type: "uuid", nullable: false),
                    TemplateId = table.Column<int>(type: "integer", nullable: true),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    LayoutJson = table.Column<string>(type: "character varying(32000)", maxLength: 32000, nullable: false),
                    PreviewHtml = table.Column<string>(type: "character varying(16000)", maxLength: 16000, nullable: true),
                    PreviewUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_invitation_drafts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_invitation_drafts_events_EventId",
                        column: x => x.EventId,
                        principalTable: "events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_invitation_drafts_invitation_templates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "invitation_templates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_invitation_drafts_users_OrganizerId",
                        column: x => x.OrganizerId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "invitations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EventId = table.Column<int>(type: "integer", nullable: false),
                    OrganizerId = table.Column<Guid>(type: "uuid", nullable: false),
                    TemplateId = table.Column<int>(type: "integer", nullable: true),
                    DraftId = table.Column<int>(type: "integer", nullable: true),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    LayoutJson = table.Column<string>(type: "character varying(32000)", maxLength: 32000, nullable: false),
                    PreviewHtml = table.Column<string>(type: "character varying(16000)", maxLength: 16000, nullable: true),
                    PreviewUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_invitations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_invitations_events_EventId",
                        column: x => x.EventId,
                        principalTable: "events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_invitations_invitation_drafts_DraftId",
                        column: x => x.DraftId,
                        principalTable: "invitation_drafts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_invitations_invitation_templates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "invitation_templates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_invitations_users_OrganizerId",
                        column: x => x.OrganizerId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_invitation_drafts_EventId",
                table: "invitation_drafts",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_invitation_drafts_OrganizerId",
                table: "invitation_drafts",
                column: "OrganizerId");

            migrationBuilder.CreateIndex(
                name: "IX_invitation_drafts_TemplateId",
                table: "invitation_drafts",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_invitation_drafts_UpdatedAt",
                table: "invitation_drafts",
                column: "UpdatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_invitation_templates_DefaultSystem",
                table: "invitation_templates",
                column: "DefaultSystem");

            migrationBuilder.CreateIndex(
                name: "IX_invitation_templates_EventId",
                table: "invitation_templates",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_invitation_templates_Name",
                table: "invitation_templates",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_invitation_templates_OrganizerId",
                table: "invitation_templates",
                column: "OrganizerId");

            migrationBuilder.CreateIndex(
                name: "IX_invitations_CreatedAt",
                table: "invitations",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_invitations_DraftId",
                table: "invitations",
                column: "DraftId");

            migrationBuilder.CreateIndex(
                name: "IX_invitations_EventId",
                table: "invitations",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_invitations_OrganizerId",
                table: "invitations",
                column: "OrganizerId");

            migrationBuilder.CreateIndex(
                name: "IX_invitations_TemplateId",
                table: "invitations",
                column: "TemplateId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "invitations");

            migrationBuilder.DropTable(
                name: "invitation_drafts");

            migrationBuilder.DropTable(
                name: "invitation_templates");
        }
    }
}
