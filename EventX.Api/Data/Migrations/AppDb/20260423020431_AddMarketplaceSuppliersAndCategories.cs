using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EventX.Api.Data.Migrations.AppDb
{
    /// <inheritdoc />
    public partial class AddMarketplaceSuppliersAndCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "supplier_categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplier_categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "suppliers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    SupplierCategoryId = table.Column<int>(type: "integer", nullable: false),
                    City = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    State = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    Description = table.Column<string>(type: "character varying(2500)", maxLength: 2500, nullable: false),
                    ImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PriceMin = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    PriceMax = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    Rating = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 0m),
                    ReviewCount = table.Column<int>(type: "integer", nullable: false),
                    AcceptanceRate = table.Column<decimal>(type: "numeric(6,4)", precision: 6, scale: 4, nullable: false, defaultValue: 0m),
                    ResponseRate = table.Column<decimal>(type: "numeric(6,4)", precision: 6, scale: 4, nullable: false, defaultValue: 0m),
                    CancellationRate = table.Column<decimal>(type: "numeric(6,4)", precision: 6, scale: 4, nullable: false, defaultValue: 0m),
                    PunctualityScore = table.Column<decimal>(type: "numeric(6,4)", precision: 6, scale: 4, nullable: false, defaultValue: 0m),
                    Featured = table.Column<bool>(type: "boolean", nullable: false),
                    RankingPosition = table.Column<int>(type: "integer", nullable: true),
                    RecentPerformanceScore = table.Column<decimal>(type: "numeric(6,4)", precision: 6, scale: 4, nullable: false, defaultValue: 0m),
                    PopularityScore = table.Column<decimal>(type: "numeric(6,4)", precision: 6, scale: 4, nullable: false, defaultValue: 0m),
                    TotalHires = table.Column<int>(type: "integer", nullable: false),
                    Premium = table.Column<bool>(type: "boolean", nullable: false),
                    BadgesJson = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false, defaultValue: "[]"),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_suppliers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_suppliers_supplier_categories_SupplierCategoryId",
                        column: x => x.SupplierCategoryId,
                        principalTable: "supplier_categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_supplier_categories_IsActive",
                table: "supplier_categories",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_supplier_categories_Name",
                table: "supplier_categories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplier_categories_SortOrder",
                table: "supplier_categories",
                column: "SortOrder");

            migrationBuilder.CreateIndex(
                name: "IX_suppliers_City",
                table: "suppliers",
                column: "City");

            migrationBuilder.CreateIndex(
                name: "IX_suppliers_Featured",
                table: "suppliers",
                column: "Featured");

            migrationBuilder.CreateIndex(
                name: "IX_suppliers_IsActive",
                table: "suppliers",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_suppliers_Name",
                table: "suppliers",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_suppliers_State",
                table: "suppliers",
                column: "State");

            migrationBuilder.CreateIndex(
                name: "IX_suppliers_SupplierCategoryId",
                table: "suppliers",
                column: "SupplierCategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "suppliers");

            migrationBuilder.DropTable(
                name: "supplier_categories");
        }
    }
}
