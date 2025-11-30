# EventBotService - Assistente Virtual do EventX

## ?? Visão Geral

O `EventBotService` é um serviço de assistente virtual integrado ao projeto EventX que utiliza a API do Google Gemini para fornecer respostas inteligentes e contextsuais sobre eventos.

## ?? Funcionalidades

### 1. Processamento de Perguntas
- Responde perguntas sobre eventos específicos
- Utiliza contexto do evento (convidados, despesas, tarefas, etc.)
- Salva interações no banco de dados

### 2. Geração de Sugestões
- Analisa dados do evento
- Fornece 3 sugestões práticas para melhorar a organização
- Considera todos os aspectos do evento (orçamento, tarefas, convidados)

### 3. Análise de Orçamento
- Avalia gastos atuais vs. custo estimado
- Calcula percentual gasto
- Fornece recomendações financeiras

## ?? Configuração

### Variáveis de Ambiente
Certifique-se de que a chave da API do Gemini está configurada no arquivo `.env`:

```
GEMINI_API_KEY=sua_chave_api_aqui
```

### Registro do Serviço
O serviço está registrado no `Program.cs`:

```csharp
builder.Services.AddHttpClient();
builder.Services.AddScoped<EventBotService>();
```

## ?? Como Usar

### No Controller

```csharp
public class MeuController : Controller
{
    private readonly EventBotService _eventBotService;
    
    public MeuController(EventBotService eventBotService)
    {
        _eventBotService = eventBotService;
    }
    
    public async Task<IActionResult> FazerPergunta(string pergunta, int? eventoId)
    {
        var resposta = await _eventBotService.ProcessarPerguntaAsync(pergunta, eventoId);
        return Json(new { resposta });
    }
}
```

### Métodos Disponíveis

#### 1. ProcessarPerguntaAsync
```csharp
var resposta = await _eventBotService.ProcessarPerguntaAsync(
    "Como está o progresso do meu evento?", 
    eventoId: 123, 
    userId: 456
);
```

#### 2. GerarSugestaoEventoAsync
```csharp
var sugestoes = await _eventBotService.GerarSugestaoEventoAsync(eventoId: 123);
```

#### 3. AnalisarOrcamentoAsync
```csharp
var analise = await _eventBotService.AnalisarOrcamentoAsync(eventoId: 123);
```

## ?? Exemplos de Uso

### Perguntas que o Assistente pode Responder:

- **Progresso do Evento**: "Como está o andamento do meu casamento?"
- **Status de Convidados**: "Quantos convidados confirmaram presença?"
- **Tarefas Pendentes**: "O que ainda preciso fazer para o evento?"
- **Análise de Orçamento**: "Estou gastando muito?"
- **Sugestões**: "Como posso melhorar meu evento?"
- **Logística**: "Preciso contratar mais fornecedores?"

### Contexto Fornecido ao Assistente:

O assistente tem acesso a informações como:
- Nome, data e local do evento
- Lista de convidados e confirmações
- Despesas e orçamento
- Tarefas pendentes e concluídas
- Pedidos realizados
- Tipo e descrição do evento

## ?? Personalização

### Modificar o Prompt do Assistente
Para personalizar as respostas, edite o método `ConstruirPrompt` no `EventBotService.cs`:

```csharp
private string ConstruirPrompt(string pergunta, string contextoEvento, string contextoUsuario)
{
    var prompt = new StringBuilder();
    prompt.AppendLine("Sua personalização aqui...");
    // ... resto do código
}
```

### Adicionar Novos Tipos de Análise
Crie novos métodos seguindo o padrão:

```csharp
public async Task<string> NovaAnaliseAsync(int eventoId)
{
    var evento = await _context.Eventos
        .Include(e => e.PropriedadeRelevante)
        .FirstOrDefaultAsync(e => e.Id == eventoId);
        
    var prompt = "Seu prompt personalizado...";
    return await ChamarGeminiApiAsync(prompt);
}
```

## ?? Tratamento de Erros

O serviço inclui tratamento robusto de erros:
- Falhas na API do Gemini
- Problemas de conexão
- Dados inválidos
- Timeouts

Erros são logados mas não interrompem a aplicação.

## ?? Monitoramento

As interações são salvas na tabela `AssistentesVirtuais` para:
- Análise de uso
- Melhoria do serviço
- Histórico de conversas
- Auditoria

## ?? Segurança

- API Key protegida por variáveis de ambiente
- Validação de entrada
- Sanitização de dados
- Rate limiting (implementar se necessário)

## ?? Melhorias Futuras

1. **Cache de Respostas**: Implementar cache para perguntas frequentes
2. **Análise de Sentimentos**: Detectar satisfação do usuário
3. **Aprendizado**: Sistema de feedback para melhorar respostas
4. **Multilíngue**: Suporte a múltiplos idiomas
5. **Integração com Calendário**: Lembretes e notificações