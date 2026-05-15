using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EventX.Api.Data.Migrations.AppDb
{
    /// <inheritdoc />
    public partial class AddStoryCreationAndHighlights : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_stories_users_UserId",
                table: "stories");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "stories",
                newName: "AuthorId");

            migrationBuilder.RenameColumn(
                name: "MediaUrl",
                table: "stories",
                newName: "ImageUrl");

            migrationBuilder.RenameIndex(
                name: "IX_stories_UserId",
                table: "stories",
                newName: "IX_stories_AuthorId");

            migrationBuilder.AddColumn<string>(
                name: "CoverUrl",
                table: "highlights",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "story_views",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StoryId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ViewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_story_views", x => x.Id);
                    table.ForeignKey(
                        name: "FK_story_views_stories_StoryId",
                        column: x => x.StoryId,
                        principalTable: "stories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_story_views_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_story_views_StoryId_UserId",
                table: "story_views",
                columns: new[] { "StoryId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_story_views_UserId",
                table: "story_views",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_story_views_ViewedAt",
                table: "story_views",
                column: "ViewedAt");

            migrationBuilder.AddForeignKey(
                name: "FK_stories_users_AuthorId",
                table: "stories",
                column: "AuthorId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_stories_users_AuthorId",
                table: "stories");

            migrationBuilder.DropTable(
                name: "story_views");

            migrationBuilder.DropColumn(
                name: "CoverUrl",
                table: "highlights");

            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "stories",
                newName: "MediaUrl");

            migrationBuilder.RenameColumn(
                name: "AuthorId",
                table: "stories",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_stories_AuthorId",
                table: "stories",
                newName: "IX_stories_UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_stories_users_UserId",
                table: "stories",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
