using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ProjetoEventX.Data;
using ProjetoEventX.Models;

namespace ProjetoEventX.Services
{
    public class EventBotService
    {
        private readonly EventXContext _context;
        private readonly HttpClient _httpClient;
        private readonly string _geminiApiKey;
        private readonly string _geminiApiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent";

        public EventBotService(EventXContext context, HttpClient httpClient)
        {
            _context = context;
            _httpClient = httpClient;
            _geminiApiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY") ?? throw new InvalidOperationException("GEMINI_API_KEY não encontrada");
        }

        public async Task<string> ProcessarPerguntaAsync(string pergunta, int? eventoId = null, int? userId = null)
        {
            try
            {
                var contextoEvento = await ObterContextoEventoAsync(eventoId);
                var contextoUsuario = await ObterContextoUsuarioAsync(userId);

                var prompt = ConstruirPrompt(pergunta, contextoEvento, contextoUsuario);
                var resposta = await ChamarGeminiApiAsync(prompt);

                // Salvar a interação no banco de dados se necessário
                await SalvarInteracaoAsync(pergunta, resposta, eventoId, userId);

                return resposta;
            }
            catch (Exception ex)
            {
                return $"Desculpe, ocorreu um erro ao processar sua pergunta: {ex.Message}";
            }
        }

        private async Task<string> ObterContextoEventoAsync(int? eventoId)
        {
            if (!eventoId.HasValue)
                return "";

            var evento = await _context.Eventos
                .Include(e => e.ListasConvidados)
                .Include(e => e.Pedidos)
                .Include(e => e.Despesas)
                .Include(e => e.TarefasEventos)
                .Include(e => e.Local)
                .FirstOrDefaultAsync(e => e.Id == eventoId.Value);

            if (evento == null)
                return "";

            var contexto = new StringBuilder();
            contexto.AppendLine($"EVENTO: {evento.NomeEvento}");
            contexto.AppendLine($"Data: {evento.DataEvento:dd/MM/yyyy} de {evento.HoraInicio} às {evento.HoraFim}");
            
            if (evento.Local != null)
            {
                contexto.AppendLine($"Local: {evento.Local.NomeLocal}");
            }
            
            contexto.AppendLine($"Descrição: {evento.DescricaoEvento}");
            contexto.AppendLine($"Tipo: {evento.TipoEvento}");
            contexto.AppendLine($"Status: {evento.StatusEvento}");
            contexto.AppendLine($"Custo Estimado: R$ {evento.CustoEstimado:F2}");
            contexto.AppendLine($"Público Estimado: {evento.PublicoEstimado}");

            var totalConvidados = evento.ListasConvidados.Count;
            contexto.AppendLine($"Total de Convidados: {totalConvidados}");

            var convidadosConfirmados = evento.ListasConvidados.Count(lc => lc.ConfirmaPresenca == "Confirmado");
            contexto.AppendLine($"Convidados Confirmados: {convidadosConfirmados}");

            var totalPedidos = evento.Pedidos.Count;
            contexto.AppendLine($"Total de Pedidos: {totalPedidos}");

            var totalDespesas = evento.Despesas.Sum(d => d.Valor);
            contexto.AppendLine($"Total de Despesas: R$ {totalDespesas:F2}");

            var tarefasPendentes = evento.TarefasEventos.Count(t => t.StatusConclusao == "Pendente");
            var tarefasConcluidas = evento.TarefasEventos.Count(t => t.StatusConclusao == "Concluída");
            contexto.AppendLine($"Tarefas Pendentes: {tarefasPendentes}");
            contexto.AppendLine($"Tarefas Concluídas: {tarefasConcluidas}");

            return contexto.ToString();
        }

        private async Task<string> ObterContextoUsuarioAsync(int? userId)
        {
            if (!userId.HasValue)
                return "";

            var user = await _context.Users.FindAsync(userId.Value);
            if (user == null)
                return "";

            return $"Usuário: {user.UserName} ({user.Email})";
        }

        private string ConstruirPrompt(string pergunta, string contextoEvento, string contextoUsuario)
        {
            var prompt = new StringBuilder();
            prompt.AppendLine("Você é o assistente virtual do EventX, uma plataforma de gestão de eventos.");
            prompt.AppendLine("Sua função é ajudar os usuários com informações sobre seus eventos, dar sugestões e resolver dúvidas.");
            prompt.AppendLine("Seja sempre cordial, prestativo e direto nas respostas.");
            prompt.AppendLine("Foque em soluções práticas para gestão de eventos.");
            prompt.AppendLine();

            if (!string.IsNullOrEmpty(contextoUsuario))
            {
                prompt.AppendLine("CONTEXTO DO USUÁRIO:");
                prompt.AppendLine(contextoUsuario);
                prompt.AppendLine();
            }

            if (!string.IsNullOrEmpty(contextoEvento))
            {
                prompt.AppendLine("CONTEXTO DO EVENTO:");
                prompt.AppendLine(contextoEvento);
                prompt.AppendLine();
            }

            prompt.AppendLine($"PERGUNTA DO USUÁRIO: {pergunta}");
            prompt.AppendLine();
            prompt.AppendLine("Por favor, forneça uma resposta útil e específica baseada no contexto fornecido.");

            return prompt.ToString();
        }

        private async Task<string> ChamarGeminiApiAsync(string prompt)
        {
            try
            {
                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new { text = prompt }
                            }
                        }
                    }
                };

                var json = JsonSerializer.Serialize(requestBody);
                var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

                var url = $"{_geminiApiUrl}?key={_geminiApiKey}";
                var response = await _httpClient.PostAsync(url, httpContent);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Erro na API do Gemini: {response.StatusCode} - {errorContent}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var responseJson = JsonDocument.Parse(responseContent);

                var candidates = responseJson.RootElement.GetProperty("candidates");
                if (candidates.GetArrayLength() > 0)
                {
                    var firstCandidate = candidates[0];
                    var candidateContent = firstCandidate.GetProperty("content");
                    var parts = candidateContent.GetProperty("parts");
                    if (parts.GetArrayLength() > 0)
                    {
                        return parts[0].GetProperty("text").GetString() ?? "Não foi possível gerar uma resposta.";
                    }
                }

                return "Não foi possível gerar uma resposta. Tente novamente.";
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao chamar API do Gemini: {ex.Message}");
            }
        }

        private async Task SalvarInteracaoAsync(string pergunta, string resposta, int? eventoId, int? userId)
        {
            try
            {
                if (eventoId.HasValue)
                {
                    var evento = await _context.Eventos.FindAsync(eventoId.Value);
                    if (evento != null)
                    {
                        var assistenteVirtual = new AssistenteVirtual
                        {
                            AlgoritmoIA = "Gemini Pro",
                            Sugestoes = $"Pergunta: {pergunta}\nResposta: {resposta}",
                            EventoId = eventoId.Value,
                            Evento = evento,
                            DataGeracao = DateTime.UtcNow,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };

                        _context.AssistentesVirtuais.Add(assistenteVirtual);
                        await _context.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                // Log do erro mas não interrompe o fluxo
                Console.WriteLine($"Erro ao salvar interação: {ex.Message}");
            }
        }

        public async Task<string> GerarSugestaoEventoAsync(int eventoId)
        {
            var evento = await _context.Eventos
                .Include(e => e.ListasConvidados)
                .Include(e => e.Despesas)
                .Include(e => e.TarefasEventos)
                .FirstOrDefaultAsync(e => e.Id == eventoId);

            if (evento == null)
                return "Evento não encontrado.";

            var prompt = new StringBuilder();
            prompt.AppendLine("Analise este evento e forneça 3 sugestões práticas para melhorar a organização:");
            prompt.AppendLine($"Evento: {evento.NomeEvento}");
            prompt.AppendLine($"Data: {evento.DataEvento:dd/MM/yyyy}");
            prompt.AppendLine($"Tipo: {evento.TipoEvento}");
            prompt.AppendLine($"Custo Estimado: R$ {evento.CustoEstimado:F2}");
            
            var totalConvidados = evento.ListasConvidados.Count;
            prompt.AppendLine($"Convidados: {totalConvidados}");
            
            var totalDespesas = evento.Despesas.Sum(d => d.Valor);
            prompt.AppendLine($"Despesas até agora: R$ {totalDespesas:F2}");
            
            var tarefasPendentes = evento.TarefasEventos.Count(t => t.StatusConclusao == "Pendente");
            prompt.AppendLine($"Tarefas pendentes: {tarefasPendentes}");

            return await ChamarGeminiApiAsync(prompt.ToString());
        }

        public async Task<string> AnalisarOrcamentoAsync(int eventoId)
        {
            var evento = await _context.Eventos
                .Include(e => e.Despesas)
                .FirstOrDefaultAsync(e => e.Id == eventoId);

            if (evento == null)
                return "Evento não encontrado.";

            var totalDespesas = evento.Despesas.Sum(d => d.Valor);
            var percentualGasto = evento.CustoEstimado > 0 ? (totalDespesas / evento.CustoEstimado) * 100 : 0;

            var prompt = $"Analise este orçamento de evento e dê recomendações:\n" +
                        $"Custo estimado: R$ {evento.CustoEstimado:F2}\n" +
                        $"Gasto atual: R$ {totalDespesas:F2}\n" +
                        $"Percentual gasto: {percentualGasto:F1}%\n" +
                        $"Dias até o evento: {(evento.DataEvento - DateTime.Now).Days}\n" +
                        "Forneça análise e sugestões para gestão do orçamento.";

            return await ChamarGeminiApiAsync(prompt);
        }
    }
}
