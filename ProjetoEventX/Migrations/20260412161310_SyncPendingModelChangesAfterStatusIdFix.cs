using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjetoEventX.Migrations
{
    /// <inheritdoc />
    public partial class SyncPendingModelChangesAfterStatusIdFix : Migration
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
                        ALTER TABLE "SocialStatusVisualizacoes"
                        DROP CONSTRAINT IF EXISTS "FK_SocialStatusVisualizacoes_SocialStatus_StatusId";

                        IF EXISTS (
                            SELECT 1
                            FROM information_schema.columns
                            WHERE table_schema = 'public'
                              AND table_name = 'SocialStatusVisualizacoes'
                              AND column_name = 'StatusId'
                        ) AND NOT EXISTS (
                            SELECT 1
                            FROM information_schema.columns
                            WHERE table_schema = 'public'
                              AND table_name = 'SocialStatusVisualizacoes'
                              AND column_name = 'SocialStatusId'
                        ) THEN
                            ALTER TABLE "SocialStatusVisualizacoes"
                            RENAME COLUMN "StatusId" TO "SocialStatusId";
                        END IF;

                        DROP INDEX IF EXISTS "IX_SocialStatusVisualizacoes_StatusId_UserId";
                        CREATE UNIQUE INDEX IF NOT EXISTS "IX_SocialStatusVisualizacoes_SocialStatusId_UserId"
                            ON "SocialStatusVisualizacoes" ("SocialStatusId", "UserId");

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
                    END IF;
                END
                $$;
                """);
        }

        /// <inheritdoc />
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

                        IF EXISTS (
                            SELECT 1
                            FROM information_schema.columns
                            WHERE table_schema = 'public'
                              AND table_name = 'SocialStatusVisualizacoes'
                              AND column_name = 'StatusId'
                        ) THEN
                            ALTER TABLE "SocialStatusVisualizacoes"
                            ADD CONSTRAINT "FK_SocialStatusVisualizacoes_SocialStatus_StatusId"
                            FOREIGN KEY ("StatusId")
                            REFERENCES "SocialStatus" ("Id")
                            ON DELETE CASCADE;
                        END IF;
                    END IF;
                END
                $$;
                """);
        }
    }
}
