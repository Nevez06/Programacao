# Instruções para Configurar o Banco de Dados no Supabase

Este documento contém instruções para criar o banco de dados do projeto EventX no Supabase.

## Arquivos Disponíveis

1. **database_schema.sql** - Script principal com toda a estrutura do banco de dados
2. **database_seed.sql** - Script opcional com dados de exemplo

## Passo a Passo

### 1. Acessar o Supabase

1. Acesse [https://supabase.com](https://supabase.com)
2. Faça login na sua conta
3. Crie um novo projeto ou selecione um projeto existente

### 2. Acessar o SQL Editor

1. No painel do Supabase, clique em **SQL Editor** no menu lateral
2. Clique em **New Query** para criar uma nova query

### 3. Executar o Script Principal

1. Abra o arquivo `database_schema.sql`
2. Copie todo o conteúdo do arquivo
3. Cole no SQL Editor do Supabase
4. Clique em **Run** ou pressione `Ctrl+Enter` (Windows) / `Cmd+Enter` (Mac)

### 4. Verificar a Criação das Tabelas

1. No menu lateral, clique em **Table Editor**
2. Verifique se todas as tabelas foram criadas:
   - AspNetUsers
   - AspNetRoles
   - Pessoas
   - Organizadores
   - Fornecedores
   - Convidados
   - Eventos
   - Produtos
   - Pedidos
   - E outras...

### 5. (Opcional) Executar Dados de Exemplo

1. Se desejar inserir dados de exemplo, abra o arquivo `database_seed.sql`
2. Copie o conteúdo e execute no SQL Editor

### 6. Configurar a String de Conexão

1. No Supabase, vá em **Settings** > **Database**
2. Copie a **Connection string** (URI)
3. No seu projeto ASP.NET Core, atualize o arquivo `.env`:

```
DB_CONNECTION=Host=seu-host.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=sua-senha
```

**Importante:** Substitua `seu-host`, `sua-senha` pelos valores reais do seu projeto Supabase.

## Estrutura do Banco de Dados

### Tabelas Principais

- **AspNetUsers** - Usuários do sistema (Identity)
- **Pessoas** - Dados pessoais base
- **Organizadores** - Organizadores de eventos
- **Fornecedores** - Fornecedores de produtos/serviços
- **Convidados** - Convidados para eventos
- **Eventos** - Eventos cadastrados
- **Produtos** - Produtos dos fornecedores
- **Pedidos** - Pedidos de produtos
- **Pagamentos** - Pagamentos realizados
- **Locais** - Locais para eventos
- **TemplatesEventos** - Templates de eventos
- **TarefasEventos** - Tarefas dos eventos
- **AssistentesVirtuais** - Sugestões de IA
- **ListasConvidados** - Lista de convidados por evento
- **Notificacoes** - Notificações do sistema
- **Feedbacks** - Feedbacks sobre eventos
- **MensagensChat** - Mensagens do chat
- **Despesas** - Despesas dos eventos
- **Administracoes** - Administração financeira

## Observações Importantes

1. **Tipos de Dados**: O script corrige algumas inconsistências encontradas no código C#:
   - `Pedido.EventoId` ajustado para INTEGER (no código está como Guid)
   - `Produto.FornecedorId` ajustado para INTEGER (no código está como Guid)

2. **Triggers**: O script cria triggers automáticos para atualizar o campo `UpdatedAt` em todas as tabelas que possuem esse campo.

3. **Índices**: Foram criados índices nas chaves estrangeiras principais para melhorar a performance.

4. **Constraints**: Foram adicionadas constraints de CHECK para validar valores (ex: quantidade mínima, preço mínimo).

## Troubleshooting

### Erro ao executar o script

- Verifique se você tem permissões de administrador no projeto Supabase
- Certifique-se de que não há tabelas com os mesmos nomes já existentes
- Verifique se a extensão `uuid-ossp` está habilitada (o script tenta habilitar automaticamente)

### Erro de conexão do ASP.NET Core

- Verifique se a string de conexão está correta
- Certifique-se de que o banco de dados está acessível
- Verifique as configurações de firewall do Supabase

### Problemas com Identity

- O ASP.NET Core Identity criará automaticamente as tabelas necessárias se usar migrations
- Se preferir usar o script manual, certifique-se de que as tabelas do Identity foram criadas corretamente

## Próximos Passos

Após criar o banco de dados:

1. Execute as migrations do Entity Framework (se estiver usando):
   ```bash
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

2. Ou configure o projeto para usar o banco criado manualmente

3. Teste a conexão executando o projeto:
   ```bash
   dotnet run
   ```

## Suporte

Se encontrar problemas, verifique:
- Logs do Supabase
- Logs da aplicação ASP.NET Core
- Documentação do Supabase: https://supabase.com/docs


