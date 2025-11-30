using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjetoEventX.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarColunasCidadeUF : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Adicionar colunas Cidade e UF na tabela Pessoas se não existirem
            migrationBuilder.Sql(@"
                DO $$ 
                BEGIN 
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='Pessoas' AND column_name='Cidade') THEN
                        ALTER TABLE ""Pessoas"" ADD COLUMN ""Cidade"" character varying(100) NOT NULL DEFAULT '';
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='Pessoas' AND column_name='UF') THEN
                        ALTER TABLE ""Pessoas"" ADD COLUMN ""UF"" character varying(2) NOT NULL DEFAULT '';
                    END IF;
                END $$;
            ");

            // Adicionar colunas Cidade e UF na tabela Fornecedores se não existirem
            migrationBuilder.Sql(@"
                DO $$ 
                BEGIN 
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='Fornecedores' AND column_name='Cidade') THEN
                        ALTER TABLE ""Fornecedores"" ADD COLUMN ""Cidade"" character varying(100) NOT NULL DEFAULT '';
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='Fornecedores' AND column_name='UF') THEN
                        ALTER TABLE ""Fornecedores"" ADD COLUMN ""UF"" character varying(2) NOT NULL DEFAULT '';
                    END IF;
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE ""Pessoas"" DROP COLUMN IF EXISTS ""Cidade"";
                ALTER TABLE ""Pessoas"" DROP COLUMN IF EXISTS ""UF"";
                ALTER TABLE ""Fornecedores"" DROP COLUMN IF EXISTS ""Cidade"";
                ALTER TABLE ""Fornecedores"" DROP COLUMN IF EXISTS ""UF"";
            ");
        }
    }
}
