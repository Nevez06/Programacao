using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using ProjetoEventX.Data;

#nullable disable

namespace ProjetoEventX.Migrations
{
    [DbContext(typeof(EventXContext))]
    [Migration("20260409201500_FixSocialStatusVisualizacaoStatusColumn")]
    /// <inheritdoc />
    public partial class FixSocialStatusVisualizacaoStatusColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1
                        FROM information_schema.tables
                        WHERE table_schema = 'public'
                          AND table_name = 'SocialStatusVisualizacoes'
                    ) THEN
                        IF NOT EXISTS (
                            SELECT 1
                            FROM information_schema.columns
                            WHERE table_schema = 'public'
                              AND table_name = 'SocialStatusVisualizacoes'
                              AND column_name = 'StatusId'
                        ) THEN
                            IF EXISTS (
                                SELECT 1
                                FROM information_schema.columns
                                WHERE table_schema = 'public'
                                  AND table_name = 'SocialStatusVisualizacoes'
                                  AND column_name = 'SocialStatusId'
                            ) THEN
                                ALTER TABLE "SocialStatusVisualizacoes"
                                RENAME COLUMN "SocialStatusId" TO "StatusId";
                            ELSIF EXISTS (
                                SELECT 1
                                FROM information_schema.columns
                                WHERE table_schema = 'public'
                                  AND table_name = 'SocialStatusVisualizacoes'
                                  AND column_name = 'StoryId'
                            ) THEN
                                ALTER TABLE "SocialStatusVisualizacoes"
                                RENAME COLUMN "StoryId" TO "StatusId";
                            ELSIF EXISTS (
                                SELECT 1
                                FROM information_schema.columns
                                WHERE table_schema = 'public'
                                  AND table_name = 'SocialStatusVisualizacoes'
                                  AND column_name = 'status_id'
                            ) THEN
                                ALTER TABLE "SocialStatusVisualizacoes"
                                RENAME COLUMN "status_id" TO "StatusId";
                            ELSIF EXISTS (
                                SELECT 1
                                FROM information_schema.columns
                                WHERE table_schema = 'public'
                                  AND table_name = 'SocialStatusVisualizacoes'
                                  AND column_name = 'statusid'
                            ) THEN
                                ALTER TABLE "SocialStatusVisualizacoes"
                                RENAME COLUMN "statusid" TO "StatusId";
                            ELSE
                                RAISE EXCEPTION 'No compatible status column found in SocialStatusVisualizacoes.';
                            END IF;
                        END IF;

                        IF NOT EXISTS (
                            SELECT 1
                            FROM pg_constraint c
                            JOIN pg_class t ON t.oid = c.conrelid
                            WHERE t.relname = 'SocialStatusVisualizacoes'
                              AND c.conname = 'FK_SocialStatusVisualizacoes_SocialStatuses_StatusId'
                        ) THEN
                            ALTER TABLE "SocialStatusVisualizacoes"
                            ADD CONSTRAINT "FK_SocialStatusVisualizacoes_SocialStatuses_StatusId"
                            FOREIGN KEY ("StatusId")
                            REFERENCES "SocialStatuses" ("Id")
                            ON DELETE CASCADE;
                        END IF;

                        CREATE INDEX IF NOT EXISTS "IX_SocialStatusVisualizacoes_StatusId_UserId"
                            ON "SocialStatusVisualizacoes" ("StatusId", "UserId");
                    END IF;
                END
                $$;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
