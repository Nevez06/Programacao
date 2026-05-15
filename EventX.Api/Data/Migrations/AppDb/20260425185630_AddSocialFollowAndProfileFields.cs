using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EventX.Api.Data.Migrations.AppDb
{
    /// <inheritdoc />
    public partial class AddSocialFollowAndProfileFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "social_profiles",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "social_profiles",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Instagram",
                table: "social_profiles",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Site",
                table: "social_profiles",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "social_follows",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FollowerUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    FollowingProfileId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_social_follows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_social_follows_social_profiles_FollowingProfileId",
                        column: x => x.FollowingProfileId,
                        principalTable: "social_profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_social_follows_users_FollowerUserId",
                        column: x => x.FollowerUserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_social_follows_CreatedAt",
                table: "social_follows",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_social_follows_FollowerUserId_FollowingProfileId",
                table: "social_follows",
                columns: new[] { "FollowerUserId", "FollowingProfileId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_social_follows_FollowingProfileId",
                table: "social_follows",
                column: "FollowingProfileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "social_follows");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "social_profiles");

            migrationBuilder.DropColumn(
                name: "City",
                table: "social_profiles");

            migrationBuilder.DropColumn(
                name: "Instagram",
                table: "social_profiles");

            migrationBuilder.DropColumn(
                name: "Site",
                table: "social_profiles");
        }
    }
}
