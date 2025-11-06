-- ============================================
-- Script de Seed (Dados de Exemplo)
-- Execute após criar o schema principal
-- ============================================

-- Inserir Templates de Evento
INSERT INTO "TemplatesEventos" ("TituloTemplateEvento", "TipoEstilo", "Categoria", "CreatedAt", "UpdatedAt")
VALUES 
    ('Casamento Clássico', 'Elegante', 'Casamento'),
    ('Aniversário Infantil', 'Colorido', 'Aniversário'),
    ('Evento Corporativo', 'Profissional', 'Corporativo'),
    ('Formatura', 'Formal', 'Educacional')
ON CONFLICT DO NOTHING;

-- Inserir Locais de Exemplo
INSERT INTO "Locais" ("NomeLocal", "EnderecoLocal", "Capacidade", "TipoLocal", "CreatedAt", "UpdatedAt")
VALUES 
    ('Salão de Festas Jardim', 'Rua das Flores, 123', 200, 'Salão'),
    ('Centro de Convenções', 'Av. Principal, 456', 500, 'Centro de Eventos'),
    ('Clube Social', 'Rua do Clube, 789', 150, 'Clube'),
    ('Espaço ao Ar Livre', 'Parque Central', 300, 'Externo')
ON CONFLICT DO NOTHING;

-- Nota: Para inserir usuários, organizadores, fornecedores e convidados,
-- você precisará criar primeiro os usuários através do sistema de autenticação
-- do ASP.NET Core Identity, que criará os registros em AspNetUsers automaticamente.

-- Exemplo de como criar um usuário organizador manualmente (após criar o usuário no Identity):
-- 1. Primeiro crie a Pessoa
-- INSERT INTO "Pessoas" ("Nome", "Email", "CreatedAt", "UpdatedAt")
-- VALUES ('João Silva', 'joao@example.com', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)
-- RETURNING "Id";

-- 2. Depois crie o Organizador (substitua o Id do usuário criado pelo Identity)
-- INSERT INTO "Organizadores" ("Id", "PessoaId", "CreatedAt", "UpdatedAt")
-- VALUES (1, 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP);


