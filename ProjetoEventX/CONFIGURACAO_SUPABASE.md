# Configuração do Banco de Dados Supabase - ProjetoEventX

## Credenciais Configuradas

- **Host:** onxcyxoeryrlfkihcrix.supabase.co
- **Porta:** 5432
- **Banco de Dados:** postgres (padrão do Supabase)
- **Usuário:** postgres
- **Senha:** Kosnika020312#
- **URL:** https://onxcyxoeryrlfkihcrix.supabase.co

## String de Conexão

A string de conexão está configurada no arquivo `.env`:

```
DB_CONNECTION=Host=onxcyxoeryrlfkihcrix.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=Kosnika020312#
```

## Chaves do Supabase

### Anon Key (Pública)
```
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Im9ueGN5eG9lcnlybGZraWhjcml4Iiwicm9sZSI6ImFub24iLCJpYXQiOjE3NTc4NzU2ODAsImV4cCI6MjA3MzQ1MTY4MH0.Cplp8vID8NsG1yGtVtXc1QbVIaaAOxefKPbYVnmewTw
```

### Service Role Key (Secreta)
```
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Im9ueGN5eG9lcnlybGZraWhjcml4Iiwicm9sZSI6InNlcnZpY2Vfcm9sZSIsImlhdCI6MTc1Nzg3NTY4MCwiZXhwIjoyMDczNDUxNjgwfQ.qReljgMIkSjCmMTOyxgjEZrOSYm6dIZBpbP1DJVS4Oc
```

## Passos para Configuração Completa

### 1. Executar o Script SQL no Supabase

1. Acesse: https://onxcyxoeryrlfkihcrix.supabase.co
2. Vá em **SQL Editor**
3. Clique em **New Query**
4. Abra o arquivo `database_schema.sql` do projeto
5. Copie e cole todo o conteúdo
6. Clique em **Run** para executar

### 2. Verificar as Tabelas Criadas

1. No Supabase, vá em **Table Editor**
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

### 3. Testar a Conexão

Execute o projeto:

```bash
cd ProjetoEventX
dotnet run
```

Se houver erros de conexão, verifique:
- Se o script SQL foi executado completamente
- Se o nome do banco está correto (pode ser `postgres` ou `ProjetoEventX-Db`)
- Se as credenciais estão corretas no arquivo `.env`

## Nota sobre o Nome do Banco

Você mencionou que o nome do banco é `ProjetoEventX-Db`. No Supabase, o banco padrão é `postgres`. 

**Se você criou um banco específico chamado `ProjetoEventX-Db`:**
- Atualize o arquivo `.env` alterando `Database=postgres` para `Database=ProjetoEventX-Db`

**Se você está usando o banco padrão:**
- Mantenha como está (`Database=postgres`)

## Troubleshooting

### Erro: "Database does not exist"
- Verifique se o nome do banco está correto
- No Supabase, todos os projetos usam o banco `postgres` por padrão

### Erro: "Password authentication failed"
- Verifique se a senha está correta: `Kosnika020312#`
- Certifique-se de que não há espaços extras na string de conexão

### Erro: "Connection timeout"
- Verifique se o projeto Supabase está ativo
- Verifique as configurações de firewall no Supabase

## Arquivos Importantes

- `.env` - Configurações de conexão (já configurado)
- `database_schema.sql` - Script para criar as tabelas
- `database_seed.sql` - Dados de exemplo (opcional)
- `INSTRUCOES_BANCO_DADOS.md` - Instruções detalhadas

## Próximos Passos

1. ✅ Arquivo `.env` configurado
2. ⏳ Executar `database_schema.sql` no Supabase
3. ⏳ Testar conexão executando `dotnet run`
4. ⏳ Criar usuários através do sistema de autenticação


