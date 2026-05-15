using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using ProjetoEventX.Data;

#nullable disable

namespace ProjetoEventX.Migrations
{
    [DbContext(typeof(EventXContext))]
    [Migration("20260411232043_FixSocialStatusVisualizacoesColumnName")]
    public partial class FixSocialStatusVisualizacoesColumnName : Migration
    {
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
                              AND column_name = 'SocialStatusId'
                        ) THEN
                            IF EXISTS (
                                SELECT 1
                                FROM information_schema.columns
                                WHERE table_schema = 'public'
                                  AND table_name = 'SocialStatusVisualizacoes'
                                  AND column_name = 'StatusId'
                            ) THEN
                                ALTER TABLE "SocialStatusVisualizacoes"
                                RENAME COLUMN "StatusId" TO "SocialStatusId";
                            ELSIF EXISTS (
                                SELECT 1
                                FROM information_schema.columns
                                WHERE table_schema = 'public'
                                  AND table_name = 'SocialStatusVisualizacoes'
                                  AND column_name = 'StoryId'
                            ) THEN
                                ALTER TABLE "SocialStatusVisualizacoes"
                                RENAME COLUMN "StoryId" TO "SocialStatusId";
                            ELSIF EXISTS (
                                SELECT 1
                                FROM information_schema.columns
                                WHERE table_schema = 'public'
                                  AND table_name = 'SocialStatusVisualizacoes'
                                  AND column_name = 'status_id'
                            ) THEN
                                ALTER TABLE "SocialStatusVisualizacoes"
                                RENAME COLUMN "status_id" TO "SocialStatusId";
                            ELSIF EXISTS (
                                SELECT 1
                                FROM information_schema.columns
                                WHERE table_schema = 'public'
                                  AND table_name = 'SocialStatusVisualizacoes'
                                  AND column_name = 'statusid'
                            ) THEN
                                ALTER TABLE "SocialStatusVisualizacoes"
                                RENAME COLUMN "statusid" TO "SocialStatusId";
                            END IF;
                        END IF;

                        ALTER TABLE "SocialStatusVisualizacoes"
                        DROP CONSTRAINT IF EXISTS "FK_SocialStatusVisualizacoes_SocialStatuses_StatusId";

                        ALTER TABLE "SocialStatusVisualizacoes"
                        DROP CONSTRAINT IF EXISTS "FK_SocialStatusVisualizacoes_SocialStatus_StatusId";

                        ALTER TABLE "SocialStatusVisualizacoes"
                        DROP CONSTRAINT IF EXISTS "FK_SocialStatusVisualizacoes_SocialStatuses_SocialStatusId";

                        IF EXISTS (
                            SELECT 1
                            FROM information_schema.columns
                            WHERE table_schema = 'public'
                              AND table_name = 'SocialStatusVisualizacoes'
                              AND column_name = 'SocialStatusId'
                        ) THEN
                            ALTER TABLE "SocialStatusVisualizacoes"
                            DROP CONSTRAINT IF EXISTS "FK_SocialStatusVisualizacoes_SocialStatus_SocialStatusId";

                            ALTER TABLE "SocialStatusVisualizacoes"
                            ADD CONSTRAINT "FK_SocialStatusVisualizacoes_SocialStatus_SocialStatusId"
                            FOREIGN KEY ("SocialStatusId")
                            REFERENCES "SocialStatus" ("Id")
                            ON DELETE CASCADE;
                        END IF;

                        DROP INDEX IF EXISTS "IX_SocialStatusVisualizacoes_StatusId_UserId";
                        CREATE UNIQUE INDEX IF NOT EXISTS "IX_SocialStatusVisualizacoes_SocialStatusId_UserId"
                            ON "SocialStatusVisualizacoes" ("SocialStatusId", "UserId");
                    END IF;
                END
                $$;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
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
                        ALTER TABLE "SocialStatusVisualizacoes"
                        DROP CONSTRAINT IF EXISTS "FK_SocialStatusVisualizacoes_SocialStatus_SocialStatusId";

                        ALTER TABLE "SocialStatusVisualizacoes"
                        DROP CONSTRAINT IF EXISTS "FK_SocialStatusVisualizacoes_SocialStatus_StatusId";

                        IF EXISTS (
                            SELECT 1
                            FROM information_schema.columns
                            WHERE table_schema = 'public'
                              AND table_name = 'SocialStatusVisualizacoes'
                              AND column_name = 'SocialStatusId'
                        ) AND NOT EXISTS (
                            SELECT 1
                            FROM information_schema.columns
                            WHERE table_schema = 'public'
                              AND table_name = 'SocialStatusVisualizacoes'
                              AND column_name = 'StatusId'
                        ) THEN
                            ALTER TABLE "SocialStatusVisualizacoes"
                            RENAME COLUMN "SocialStatusId" TO "StatusId";
                        END IF;

                        DROP INDEX IF EXISTS "IX_SocialStatusVisualizacoes_SocialStatusId_UserId";
                        CREATE INDEX IF NOT EXISTS "IX_SocialStatusVisualizacoes_StatusId_UserId"
                            ON "SocialStatusVisualizacoes" ("StatusId", "UserId");
                    END IF;
                END
                $$;
                """);
        }
    }
}
