# 🔐 EvenTX - Sistema de Gerenciamento de Eventos com Segurança Extrema

## 📋 Visão Geral

O **EvenTX** é uma plataforma inteligente e ultra-segura para organização de eventos sociais e corporativos. Desenvolvido com foco extremo na segurança, o sistema centraliza todo o planejamento de eventos em uma única interface robusta e protegida.

### 🎯 Objetivos
- Centralizar o planejamento de eventos
- Proteger dados sensíveis com segurança extrema
- Gerenciar convidados com templates personalizados
- Controlar acessos e permissões
- Fornecer auditoria completa de todas as operações

## 🔒 Características de Segurança

### 🛡️ Segurança Extrema Implementada
- **Headers de Segurança**: Proteção contra XSS, CSRF, clickjacking
- **Validação de Entrada**: Sanitização completa contra injeção de código
- **Bloqueio de Conta**: Após 5 tentativas de login falhadas
- **Auditoria Completa**: Todos os acessos e operações são registrados
- **Criptografia**: Senhas com requisitos complexos (maiúsculas, minúsculas, números, caracteres especiais)
- **Rate Limiting**: Prevenção contra ataques de força bruta
- **Validação de CPF**: Verificação de CPF válido para brasileiros

### 🔐 Níveis de Acesso
1. **Organizador**: Acesso completo aos seus eventos
2. **Fornecedor**: Acesso limitado ao dashboard de fornecedor
3. **Convidado**: Acesso apenas para visualizar convites

## 🚀 Tecnologias Utilizadas

### Backend
- **.NET 6+** com ASP.NET Core MVC
- **Entity Framework Core** com PostgreSQL
- **Identity Framework** para autenticação
- **SignalR** para comunicação em tempo real

### Frontend
- **Bootstrap 5** para interface responsiva
- **Font Awesome** para ícones
- **jQuery** para interações
- **Validação client-side** integrada

### Segurança
- **Middleware customizado** para headers de segurança
- **Filtros de ação** para validação de entrada
- **Validação de dados** contra SQL injection e XSS
- **Auditoria completa** de todas as operações

## 📁 Estrutura do Projeto

```
ProjetoEventX/
├── Controllers/              # Controladores com segurança reforçada
│   ├── AuthController.cs     # Autenticação com validação extrema
│   ├── EventosController.cs  # Gerenciamento de eventos
│   ├── ConviteController.cs  # Sistema de convites
│   └── TemplateConviteController.cs # Templates de convites
├── Models/                   # Modelos de dados
│   ├── ApplicationUser.cs    # Usuário extendido
│   ├── LogsAcesso.cs         # Registro de acessos
│   ├── Auditoria.cs          # Sistema de auditoria
│   └── TemplateConvite.cs    # Modelo de template
├── Security/                 # Camada de segurança
│   ├── SecurityHeadersMiddleware.cs  # Headers de segurança
│   ├── SecurityActionFilter.cs       # Filtro de ações
│   ├── SecurityValidator.cs          # Validador de entrada
│   └── SecurityClaimsExtensions.cs   # Extensões de claims
├── Services/                 # Serviços
│   ├── AuditoriaService.cs  # Serviço de auditoria
│   └── [Outros serviços]
├── Views/                    # Views Razor
│   ├── Auth/                # Views de autenticação
│   ├── Convite/             # Views de convites
│   └── Shared/              # Layouts compartilhados
└── Data/
    └── EventXContextDB.cs     # Contexto do banco de dados
```

## 🔧 Configuração e Instalação

### Pré-requisitos
- .NET 6 ou superior
- PostgreSQL
- Node.js (para ferramentas de build)

### Instalação

1. **Clone o repositório**
```bash
git clone [url-do-repositorio]
cd ProjetoEventX
```

2. **Configure o banco de dados**
```bash
# Configure a connection string no appsettings.json
# Ou use variáveis de ambiente no arquivo .env
```

3. **Instale as dependências**
```bash
dotnet restore
```

4. **Execute as migrações**
```bash
dotnet ef database update
```

5. **Inicie a aplicação**
```bash
dotnet run
```

## 📋 Funcionalidades Principais

### 🎉 Gerenciamento de Eventos
- Criar, editar e excluir eventos
- Definir data, horário, local e capacidade
- Adicionar descrição e tipo do evento
- Status do evento (Planejado, Em Andamento, Concluído)

### 👥 Gestão de Convidados
- Adicionar convidados com validação de email
- Enviar convites personalizados
- Confirmar presença via link único
- Gerenciar status de presença (Pendente, Confirmado, Cancelado)

### 📧 Templates de Convites
- Criar templates personalizados
- Definir cores, fontes e layout
- Adicionar imagens e logotipos
- Preview antes de enviar

### 📊 Dashboard Organizador
- Visualizar estatísticas do evento
- Gerenciar múltiplos eventos
- Acompanhar confirmações
- Exportar relatórios

### 🔐 Segurança Avançada
- Autenticação com bloqueio de conta
- Validação de CPF brasileiro
- Proteção contra SQL injection
- Headers de segurança HTTP
- Auditoria completa de operações

## 🌐 URLs e Endpoints Principais

### Acesso Público
- `/` - Home do sistema
- `/Auth/LoginOrganizador` - Login de organizador
- `/Auth/LoginFornecedor` - Login de fornecedor
- `/Auth/RegistroOrganizador` - Registro de novo organizador

### Área do Organizador
- `/Eventos` - Gerenciamento de eventos
- `/Eventos/Create` - Criar novo evento
- `/Convite/Criar?eventoId=X` - Criar convite para evento
- `/TemplateConvite` - Gerenciar templates de convites
- `/Organizador/Dashboard` - Dashboard principal

### Área do Convidado
- `/Auth/LoginConvidado` - Login de convidado
- `/Convite/ConfirmarPresenca?eventoId=X&convidadoId=Y` - Confirmar presença

## 🔒 Configurações de Segurança

### Requisitos de Senha
- Mínimo 8 caracteres
- Pelo menos 1 letra maiúscula
- Pelo menos 1 letra minúscula
- Pelo menos 1 número
- Pelo menos 1 caractere especial
- 4 caracteres únicos obrigatórios

### Bloqueio de Conta
- 5 tentativas de login falhadas
- Bloqueio por 30 minutos
- Registro em log de auditoria

### Headers de Segurança
```
X-Frame-Options: DENY
X-Content-Type-Options: nosniff
X-XSS-Protection: 1; mode=block
Content-Security-Policy: [política restritiva]
Strict-Transport-Security: max-age=31536000
```

## 🚨 Monitoramento e Auditoria

### Logs de Acesso
- Todos os acessos são registrados com IP e timestamp
- Monitoramento de tentativas suspeitas
- Rate limiting implementado

### Auditoria de Operações
- CREATE, UPDATE, DELETE de todas as entidades
- Visualizações de dados sensíveis
- Login e logout de usuários
- Dados antigos e novos em cada alteração

## 📊 Performance e Escalabilidade

### Otimizações
- Consultas otimizadas com Entity Framework
- Índices de banco de dados
- Cache distribuído via sessão
- Lazy loading desabilitado

### Limites de Sistema
- Máximo 100 requisições por IP por minuto
- Upload de arquivos limitado a 10MB
- Sessão expira em 30 minutos
- Auditoria mantém histórico de 1 ano

## 🛠️ Manutenção e Suporte

### Backup
- Backup diário automático do banco de dados
- Exportação de auditoria para arquivo
- Logs rotativos por data

### Atualizações
- Migrações de banco de dados automatizadas
- Versionamento de API
- Compatibilidade retroativa

## 📞 Suporte Técnico

### Contato
- Email: suporte@eventx.com.br
- Telefone: (11) 9999-9999
- Horário: Seg-Sex 9h-18h

### Documentação
- Wiki interna do sistema
- Manual do usuário
- Vídeos tutoriais

## 📄 Licença

Este projeto é de uso exclusivo e contém propriedade intelectual protegida. 

### Termos de Uso
- Uso comercial permitido apenas com licença
- Não pode ser redistribuído sem autorização
- Código-fonte protegido por direitos autorais

## 🔄 Status do Projeto

✅ **EM PRODUÇÃO** - Sistema ativo e operacional

### Última Atualização
- Versão: 2.0
- Data: Fevereiro 2026
- Status: Segurança extrema implementada

---

**🔐 EvenTX - Organize seus eventos com segurança máxima!**