using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjetoEventX.Migrations
{
    /// <inheritdoc />
    public partial class AddSocialPostActionFlags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SocialPosts_PerfilSocialId",
                table: "SocialPosts");

            migrationBuilder.AddColumn<bool>(
                name: "CommentsEnabled",
                table: "SocialPosts",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "HideLikesCount",
                table: "SocialPosts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HideSharesCount",
                table: "SocialPosts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "SocialPosts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "SocialPosts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPinned",
                table: "SocialPosts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "PinnedOrder",
                table: "SocialPosts",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SocialPosts_PerfilSocialId_IsPinned_PinnedOrder",
                table: "SocialPosts",
                columns: new[] { "PerfilSocialId", "IsPinned", "PinnedOrder" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SocialPosts_PerfilSocialId_IsPinned_PinnedOrder",
                table: "SocialPosts");

            migrationBuilder.DropColumn(
                name: "CommentsEnabled",
                table: "SocialPosts");

            migrationBuilder.DropColumn(
                name: "HideLikesCount",
                table: "SocialPosts");

            migrationBuilder.DropColumn(
                name: "HideSharesCount",
                table: "SocialPosts");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "SocialPosts");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "SocialPosts");

            migrationBuilder.DropColumn(
                name: "IsPinned",
                table: "SocialPosts");

            migrationBuilder.DropColumn(
                name: "PinnedOrder",
                table: "SocialPosts");

            migrationBuilder.CreateIndex(
                name: "IX_SocialPosts_PerfilSocialId",
                table: "SocialPosts",
                column: "PerfilSocialId");
        }
    }
}
