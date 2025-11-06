-- ============================================
-- Script SQL para criar o banco de dados EventX no Supabase
-- Baseado nos modelos do projeto ASP.NET Core
-- ============================================

-- Habilitar extensões necessárias
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- ============================================
-- TABELAS DO IDENTITY (ASP.NET Core Identity)
-- ============================================

-- Tabela de Usuários (Identity)
CREATE TABLE IF NOT EXISTS "AspNetUsers" (
    "Id" SERIAL PRIMARY KEY,
    "UserName" VARCHAR(256),
    "NormalizedUserName" VARCHAR(256),
    "Email" VARCHAR(256),
    "NormalizedEmail" VARCHAR(256),
    "EmailConfirmed" BOOLEAN NOT NULL DEFAULT FALSE,
    "PasswordHash" TEXT,
    "SecurityStamp" TEXT,
    "ConcurrencyStamp" TEXT,
    "PhoneNumber" TEXT,
    "PhoneNumberConfirmed" BOOLEAN NOT NULL DEFAULT FALSE,
    "TwoFactorEnabled" BOOLEAN NOT NULL DEFAULT FALSE,
    "LockoutEnd" TIMESTAMP WITH TIME ZONE,
    "LockoutEnabled" BOOLEAN NOT NULL DEFAULT FALSE,
    "AccessFailedCount" INTEGER NOT NULL DEFAULT 0,
    "TipoUsuario" VARCHAR(50) -- "Organizador", "Fornecedor", "Convidado"
);

-- Tabela de Roles
CREATE TABLE IF NOT EXISTS "AspNetRoles" (
    "Id" SERIAL PRIMARY KEY,
    "Name" VARCHAR(256),
    "NormalizedName" VARCHAR(256),
    "ConcurrencyStamp" TEXT
);

-- Tabelas de relacionamento Identity
CREATE TABLE IF NOT EXISTS "AspNetUserRoles" (
    "UserId" INTEGER NOT NULL,
    "RoleId" INTEGER NOT NULL,
    PRIMARY KEY ("UserId", "RoleId"),
    FOREIGN KEY ("UserId") REFERENCES "AspNetUsers"("Id") ON DELETE CASCADE,
    FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles"("Id") ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS "AspNetUserClaims" (
    "Id" SERIAL PRIMARY KEY,
    "UserId" INTEGER NOT NULL,
    "ClaimType" TEXT,
    "ClaimValue" TEXT,
    FOREIGN KEY ("UserId") REFERENCES "AspNetUsers"("Id") ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS "AspNetUserLogins" (
    "LoginProvider" TEXT NOT NULL,
    "ProviderKey" TEXT NOT NULL,
    "ProviderDisplayName" TEXT,
    "UserId" INTEGER NOT NULL,
    PRIMARY KEY ("LoginProvider", "ProviderKey"),
    FOREIGN KEY ("UserId") REFERENCES "AspNetUsers"("Id") ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS "AspNetRoleClaims" (
    "Id" SERIAL PRIMARY KEY,
    "RoleId" INTEGER NOT NULL,
    "ClaimType" TEXT,
    "ClaimValue" TEXT,
    FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles"("Id") ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS "AspNetUserTokens" (
    "UserId" INTEGER NOT NULL,
    "LoginProvider" TEXT NOT NULL,
    "Name" TEXT NOT NULL,
    "Value" TEXT,
    PRIMARY KEY ("UserId", "LoginProvider", "Name"),
    FOREIGN KEY ("UserId") REFERENCES "AspNetUsers"("Id") ON DELETE CASCADE
);

-- Índices para Identity
CREATE INDEX IF NOT EXISTS "EmailIndex" ON "AspNetUsers"("NormalizedEmail");
CREATE UNIQUE INDEX IF NOT EXISTS "UserNameIndex" ON "AspNetUsers"("NormalizedUserName");
CREATE UNIQUE INDEX IF NOT EXISTS "RoleNameIndex" ON "AspNetRoles"("NormalizedName");
CREATE INDEX IF NOT EXISTS "IX_AspNetUserRoles_RoleId" ON "AspNetUserRoles"("RoleId");

-- ============================================
-- TABELAS DO DOMÍNIO
-- ============================================

-- Tabela Pessoas
CREATE TABLE IF NOT EXISTS "Pessoas" (
    "Id" SERIAL PRIMARY KEY,
    "Nome" VARCHAR(255) NOT NULL,
    "Endereco" VARCHAR(255),
    "Telefone" INTEGER,
    "Cpf" VARCHAR(14),
    "Email" VARCHAR(255) NOT NULL,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Tabela Locais
CREATE TABLE IF NOT EXISTS "Locais" (
    "Id" SERIAL PRIMARY KEY,
    "NomeLocal" VARCHAR(255) NOT NULL,
    "EnderecoLocal" VARCHAR(255) NOT NULL,
    "Capacidade" INTEGER NOT NULL,
    "TipoLocal" VARCHAR(100),
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Tabela Templates de Evento
CREATE TABLE IF NOT EXISTS "TemplatesEventos" (
    "Id" SERIAL PRIMARY KEY,
    "TituloTemplateEvento" VARCHAR(255) NOT NULL,
    "DataCriacao" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "TipoEstilo" VARCHAR(100),
    "Categoria" VARCHAR(100),
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Tabela Organizadores (herda de AspNetUsers)
CREATE TABLE IF NOT EXISTS "Organizadores" (
    "Id" INTEGER PRIMARY KEY REFERENCES "AspNetUsers"("Id") ON DELETE CASCADE,
    "PessoaId" INTEGER NOT NULL,
    "DataCadastro" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY ("PessoaId") REFERENCES "Pessoas"("Id")
);

-- Tabela Fornecedores (herda de AspNetUsers)
CREATE TABLE IF NOT EXISTS "Fornecedores" (
    "Id" INTEGER PRIMARY KEY REFERENCES "AspNetUsers"("Id") ON DELETE CASCADE,
    "PessoaId" INTEGER NOT NULL,
    "Cnpj" VARCHAR(18) NOT NULL,
    "TipoServico" VARCHAR(255),
    "AvaliacaoMedia" DECIMAL(18,2) NOT NULL DEFAULT 0.0,
    "DataCadastro" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY ("PessoaId") REFERENCES "Pessoas"("Id")
);

-- Tabela Convidados (herda de AspNetUsers)
CREATE TABLE IF NOT EXISTS "Convidados" (
    "Id" INTEGER PRIMARY KEY REFERENCES "AspNetUsers"("Id") ON DELETE CASCADE,
    "PessoaId" INTEGER NOT NULL,
    "ConfirmaPresenca" VARCHAR(50) DEFAULT 'Pendente',
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY ("PessoaId") REFERENCES "Pessoas"("Id")
);

-- Tabela Eventos
CREATE TABLE IF NOT EXISTS "Eventos" (
    "Id" SERIAL PRIMARY KEY,
    "NomeEvento" VARCHAR(255) NOT NULL,
    "DataEvento" TIMESTAMP NOT NULL,
    "DescricaoEvento" TEXT,
    "TipoEvento" VARCHAR(100),
    "CustoEstimado" DECIMAL(18,2) NOT NULL DEFAULT 0.0,
    "StatusEvento" VARCHAR(50) NOT NULL DEFAULT 'Planejado',
    "IdTemplateEvento" INTEGER,
    "HoraInicio" VARCHAR(5),
    "HoraFim" VARCHAR(5),
    "PublicoEstimado" INTEGER NOT NULL DEFAULT 0,
    "OrganizadorId" INTEGER NOT NULL,
    "LocalId" INTEGER,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY ("IdTemplateEvento") REFERENCES "TemplatesEventos"("Id"),
    FOREIGN KEY ("OrganizadorId") REFERENCES "Organizadores"("Id"),
    FOREIGN KEY ("LocalId") REFERENCES "Locais"("Id")
);

-- Índice para Eventos
CREATE INDEX IF NOT EXISTS "IX_Eventos_OrganizadorId" ON "Eventos"("OrganizadorId");

-- Tabela Produtos
-- NOTA: No código C# Produto.FornecedorId é Guid, mas Fornecedor.Id é int
-- Ajustado para INTEGER para corresponder ao Fornecedor.Id
CREATE TABLE IF NOT EXISTS "Produtos" (
    "Id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "Nome" VARCHAR(255) NOT NULL,
    "Descricao" TEXT,
    "Preco" DECIMAL(18,2) NOT NULL,
    "Tipo" VARCHAR(50) NOT NULL,
    "FornecedorId" INTEGER NOT NULL,
    FOREIGN KEY ("FornecedorId") REFERENCES "Fornecedores"("Id")
);

-- Tabela Pedidos
-- NOTA: No código C# Pedido.EventoId é Guid, mas Evento.Id é int
-- Ajustado para INTEGER para corresponder ao Evento.Id
CREATE TABLE IF NOT EXISTS "Pedidos" (
    "Id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "EventoId" INTEGER NOT NULL,
    "ProdutoId" UUID NOT NULL,
    "Quantidade" INTEGER NOT NULL CHECK ("Quantidade" >= 1 AND "Quantidade" <= 1000),
    "PrecoTotal" DECIMAL(18,2) NOT NULL CHECK ("PrecoTotal" >= 0.01),
    "StatusPedido" VARCHAR(50) NOT NULL DEFAULT 'Pendente',
    "DataPedido" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY ("EventoId") REFERENCES "Eventos"("Id"),
    FOREIGN KEY ("ProdutoId") REFERENCES "Produtos"("Id")
);

-- Índice para Pedidos
CREATE INDEX IF NOT EXISTS "IX_Pedidos_EventoId" ON "Pedidos"("EventoId");

-- Tabela Itens de Pedido
CREATE TABLE IF NOT EXISTS "ItensPedidos" (
    "Id" SERIAL PRIMARY KEY,
    "DescricaoItemPedido" TEXT NOT NULL,
    "Quantidade" INTEGER NOT NULL CHECK ("Quantidade" >= 1),
    "PrecoUnitario" DECIMAL(18,2) NOT NULL,
    "CategoriaItemPedido" VARCHAR(100),
    "PedidoId" UUID NOT NULL,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY ("PedidoId") REFERENCES "Pedidos"("Id") ON DELETE CASCADE
);

-- Tabela Pagamentos
CREATE TABLE IF NOT EXISTS "Pagamentos" (
    "Id" SERIAL PRIMARY KEY,
    "ValorTotal" DECIMAL(18,2) NOT NULL,
    "StatusPagamento" VARCHAR(50) NOT NULL DEFAULT 'Pendente',
    "MetodoPagamento" VARCHAR(50),
    "DataPagamento" TIMESTAMP,
    "Comprovante" VARCHAR(255),
    "PedidoId" UUID NOT NULL,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY ("PedidoId") REFERENCES "Pedidos"("Id")
);

-- Tabela Tarefas de Evento
CREATE TABLE IF NOT EXISTS "TarefasEventos" (
    "Id" SERIAL PRIMARY KEY,
    "DescricaoTarefaEvento" TEXT NOT NULL,
    "ResponsavelId" INTEGER,
    "StatusConclusao" VARCHAR(50) NOT NULL DEFAULT 'Pendente',
    "DataLimite" TIMESTAMP,
    "PrioridadeTarefaEvento" VARCHAR(50),
    "DataConclusao" TIMESTAMP,
    "EventoId" INTEGER NOT NULL,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY ("ResponsavelId") REFERENCES "Pessoas"("Id"),
    FOREIGN KEY ("EventoId") REFERENCES "Eventos"("Id")
);

-- Tabela Assistentes Virtuais
CREATE TABLE IF NOT EXISTS "AssistentesVirtuais" (
    "Id" SERIAL PRIMARY KEY,
    "AlgoritmoIA" VARCHAR(100),
    "Sugestoes" TEXT,
    "DataGeracao" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "EventoId" INTEGER NOT NULL,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY ("EventoId") REFERENCES "Eventos"("Id")
);

-- Tabela Lista de Convidados
CREATE TABLE IF NOT EXISTS "ListasConvidados" (
    "Id" SERIAL PRIMARY KEY,
    "ConvidadoId" INTEGER NOT NULL,
    "EventoId" INTEGER NOT NULL,
    "DataInclusao" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "ConfirmaPresenca" VARCHAR(50) NOT NULL DEFAULT 'Pendente',
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY ("ConvidadoId") REFERENCES "Convidados"("Id"),
    FOREIGN KEY ("EventoId") REFERENCES "Eventos"("Id"),
    UNIQUE ("ConvidadoId", "EventoId") -- Evita duplicatas
);

-- Tabela Notificações
CREATE TABLE IF NOT EXISTS "Notificacoes" (
    "Id" SERIAL PRIMARY KEY,
    "MensagemNotificacao" TEXT NOT NULL,
    "Tipo" VARCHAR(50),
    "DataEnvio" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "Lida" BOOLEAN NOT NULL DEFAULT FALSE,
    "PrioridadeNotificacao" VARCHAR(50),
    "DestinatarioId" INTEGER NOT NULL,
    "EventoId" INTEGER,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY ("DestinatarioId") REFERENCES "Pessoas"("Id"),
    FOREIGN KEY ("EventoId") REFERENCES "Eventos"("Id")
);

-- Tabela Feedbacks
CREATE TABLE IF NOT EXISTS "Feedbacks" (
    "Id" SERIAL PRIMARY KEY,
    "AvaliacaoFeedback" VARCHAR(50),
    "ComentarioFeedback" TEXT,
    "DataEnvioFeedback" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "TipoFeedback" VARCHAR(50),
    "FornecedorId" INTEGER,
    "EventoId" INTEGER,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY ("FornecedorId") REFERENCES "Fornecedores"("Id"),
    FOREIGN KEY ("EventoId") REFERENCES "Eventos"("Id")
);

-- Tabela Mensagens de Chat
CREATE TABLE IF NOT EXISTS "MensagensChat" (
    "Id" SERIAL PRIMARY KEY,
    "EventoId" INTEGER NOT NULL,
    "ConvidadoId" INTEGER NOT NULL,
    "Mensagem" TEXT NOT NULL,
    "DataEnvio" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY ("EventoId") REFERENCES "Eventos"("Id"),
    FOREIGN KEY ("ConvidadoId") REFERENCES "Convidados"("Id")
);

-- Tabela Despesas
CREATE TABLE IF NOT EXISTS "Despesas" (
    "Id" SERIAL PRIMARY KEY,
    "EventoId" INTEGER NOT NULL,
    "Descricao" TEXT NOT NULL,
    "Valor" DECIMAL(18,2) NOT NULL CHECK ("Valor" >= 0.01),
    "DataDespesa" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY ("EventoId") REFERENCES "Eventos"("Id")
);

-- Tabela Administração
CREATE TABLE IF NOT EXISTS "Administracoes" (
    "IdAdministrar" SERIAL PRIMARY KEY,
    "ValorTotal" DECIMAL(18,2) NOT NULL DEFAULT 0.0,
    "Orcamento" DECIMAL(18,2) NOT NULL DEFAULT 0.0,
    "IdEvento" INTEGER NOT NULL,
    FOREIGN KEY ("IdEvento") REFERENCES "Eventos"("Id")
);

-- ============================================
-- TRIGGERS PARA UPDATED_AT
-- ============================================

-- Função para atualizar UpdatedAt automaticamente
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW."UpdatedAt" = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ language 'plpgsql';

-- Aplicar trigger em todas as tabelas com UpdatedAt
-- Remover triggers existentes antes de criar novos
DROP TRIGGER IF EXISTS update_pessoas_updated_at ON "Pessoas";
CREATE TRIGGER update_pessoas_updated_at BEFORE UPDATE ON "Pessoas"
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

DROP TRIGGER IF EXISTS update_locais_updated_at ON "Locais";
CREATE TRIGGER update_locais_updated_at BEFORE UPDATE ON "Locais"
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

DROP TRIGGER IF EXISTS update_templates_eventos_updated_at ON "TemplatesEventos";
CREATE TRIGGER update_templates_eventos_updated_at BEFORE UPDATE ON "TemplatesEventos"
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

DROP TRIGGER IF EXISTS update_organizadores_updated_at ON "Organizadores";
CREATE TRIGGER update_organizadores_updated_at BEFORE UPDATE ON "Organizadores"
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

DROP TRIGGER IF EXISTS update_fornecedores_updated_at ON "Fornecedores";
CREATE TRIGGER update_fornecedores_updated_at BEFORE UPDATE ON "Fornecedores"
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

DROP TRIGGER IF EXISTS update_convidados_updated_at ON "Convidados";
CREATE TRIGGER update_convidados_updated_at BEFORE UPDATE ON "Convidados"
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

DROP TRIGGER IF EXISTS update_eventos_updated_at ON "Eventos";
CREATE TRIGGER update_eventos_updated_at BEFORE UPDATE ON "Eventos"
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

DROP TRIGGER IF EXISTS update_itens_pedidos_updated_at ON "ItensPedidos";
CREATE TRIGGER update_itens_pedidos_updated_at BEFORE UPDATE ON "ItensPedidos"
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

DROP TRIGGER IF EXISTS update_pagamentos_updated_at ON "Pagamentos";
CREATE TRIGGER update_pagamentos_updated_at BEFORE UPDATE ON "Pagamentos"
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

DROP TRIGGER IF EXISTS update_tarefas_eventos_updated_at ON "TarefasEventos";
CREATE TRIGGER update_tarefas_eventos_updated_at BEFORE UPDATE ON "TarefasEventos"
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

DROP TRIGGER IF EXISTS update_assistentes_virtuais_updated_at ON "AssistentesVirtuais";
CREATE TRIGGER update_assistentes_virtuais_updated_at BEFORE UPDATE ON "AssistentesVirtuais"
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

DROP TRIGGER IF EXISTS update_listas_convidados_updated_at ON "ListasConvidados";
CREATE TRIGGER update_listas_convidados_updated_at BEFORE UPDATE ON "ListasConvidados"
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

DROP TRIGGER IF EXISTS update_notificacoes_updated_at ON "Notificacoes";
CREATE TRIGGER update_notificacoes_updated_at BEFORE UPDATE ON "Notificacoes"
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

DROP TRIGGER IF EXISTS update_feedbacks_updated_at ON "Feedbacks";
CREATE TRIGGER update_feedbacks_updated_at BEFORE UPDATE ON "Feedbacks"
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

-- ============================================
-- COMENTÁRIOS NAS TABELAS
-- ============================================

COMMENT ON TABLE "AspNetUsers" IS 'Tabela de usuários do sistema (Identity)';
COMMENT ON TABLE "Pessoas" IS 'Tabela base para pessoas físicas';
COMMENT ON TABLE "Organizadores" IS 'Organizadores de eventos';
COMMENT ON TABLE "Fornecedores" IS 'Fornecedores de produtos e serviços';
COMMENT ON TABLE "Convidados" IS 'Convidados para eventos';
COMMENT ON TABLE "Eventos" IS 'Eventos cadastrados no sistema';
COMMENT ON TABLE "Produtos" IS 'Produtos oferecidos pelos fornecedores';
COMMENT ON TABLE "Pedidos" IS 'Pedidos de produtos para eventos';
COMMENT ON TABLE "Pagamentos" IS 'Pagamentos realizados';
COMMENT ON TABLE "Locais" IS 'Locais onde os eventos podem ocorrer';
COMMENT ON TABLE "TemplatesEventos" IS 'Templates para criação de eventos';
COMMENT ON TABLE "TarefasEventos" IS 'Tarefas relacionadas aos eventos';
COMMENT ON TABLE "AssistentesVirtuais" IS 'Sugestões geradas por IA para eventos';
COMMENT ON TABLE "ListasConvidados" IS 'Lista de convidados por evento';
COMMENT ON TABLE "Notificacoes" IS 'Notificações do sistema';
COMMENT ON TABLE "Feedbacks" IS 'Feedbacks sobre eventos e fornecedores';
COMMENT ON TABLE "MensagensChat" IS 'Mensagens do chat em tempo real';
COMMENT ON TABLE "Despesas" IS 'Despesas dos eventos';
COMMENT ON TABLE "Administracoes" IS 'Administração financeira dos eventos';

