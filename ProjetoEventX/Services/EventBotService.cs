using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ProjetoEventX.Data;
using ProjetoEventX.Models;
using ProjetoEventX.DTOs;

namespace ProjetoEventX.Services
{
    public class EventBotService
    {
        private readonly EventXContext _context;
        private readonly GeminiEventService _geminiEventService;
        private readonly HttpClient _httpClient;
        private readonly string _geminiApiKey;

        public EventBotService(EventXContext context, GeminiEventService geminiEventService, HttpClient httpClient)
        {
            _context = context;
            _geminiEventService = geminiEventService;
            _httpClient = httpClient;
            
            // API Key é opcional - funciona mesmo sem
            _geminiApiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY") ?? "";
            
            if (string.IsNullOrEmpty(_geminiApiKey))
            {
                Console.WriteLine("⚠️ GEMINI_API_KEY não encontrada - usando modo fallback inteligente!");
            }
            else
            {
                Console.WriteLine("✅ EventBotService: GEMINI_API_KEY carregada com sucesso!");
            }
        }

        public async Task<string> ProcessarPerguntaAsync(string pergunta, int? eventoId = null, int? userId = null)
        {
            try
            {
                // Verificar se é uma pergunta sobre criação de orçamento
                if ((pergunta.ToLower().Contains("orçamento") && pergunta.ToLower().Contains("criar")) || 
                    (pergunta.ToLower().Contains("planejar") && pergunta.ToLower().Contains("evento")) ||
                    pergunta.ToLower().Contains("sugerir fornecedor") ||
                    pergunta.ToLower().Contains("recomendar"))
                {
                    return await CriarOrcamentoInteligente(eventoId, pergunta);
                }

                // Identificar tipo de pergunta e responder com dados do banco
                var resposta = await ProcessarPerguntaComContexto(pergunta, eventoId);

                // Salvar a interação no banco de dados se necessário
                await SalvarInteracaoAsync(pergunta, resposta, eventoId, userId);

                return resposta;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erro em ProcessarPerguntaAsync: {ex.Message}");
                return $"Desculpe, ocorreu um erro ao processar sua pergunta. Mas posso ajudá-lo com informações específicas sobre seus eventos! Tente perguntar algo mais específico.";
            }
        }

        private async Task<string> ProcessarPerguntaComContexto(string pergunta, int? eventoId)
        {
            var perguntaLower = pergunta.ToLower();

            // Análise de progresso do evento
            if (perguntaLower.Contains("progresso") || perguntaLower.Contains("andamento"))
            {
                return await AnalisarProgressoEvento(eventoId);
            }

            // Status de convidados
            if (perguntaLower.Contains("convidados") || perguntaLower.Contains("confirmaram"))
            {
                return await AnalisarConvidados(eventoId);
            }

            // Tarefas pendentes
            if (perguntaLower.Contains("tarefas") || perguntaLower.Contains("pendentes"))
            {
                return await AnalisarTarefas(eventoId);
            }

            // Análise de orçamento
            if (perguntaLower.Contains("orçamento") || perguntaLower.Contains("gastos") || perguntaLower.Contains("despesas"))
            {
                return await AnalisarOrcamentoDetalhado(eventoId);
            }

            // Fornecedores
            if (perguntaLower.Contains("fornecedor"))
            {
                return await AnalisarFornecedores(eventoId);
            }

            // Economia
            if (perguntaLower.Contains("economia") || perguntaLower.Contains("economizar"))
            {
                return await DarDicasEconomia(eventoId);
            }

            // Resposta genérica com contexto
            return await RespostaGenerica(eventoId);
        }

        private async Task<string> AnalisarProgressoEvento(int? eventoId)
        {
            if (!eventoId.HasValue)
            {
                var eventosCount = await _context.Eventos.CountAsync();
                return $"📊 **Resumo Geral dos Seus Eventos**\n\n" +
                       $"Você tem **{eventosCount} eventos** cadastrados. " +
                       $"Selecione um evento específico para análises detalhadas! 🎯";
            }

            var evento = await _context.Eventos
                .Include(e => e.ListasConvidados)
                .Include(e => e.Despesas)
                .Include(e => e.TarefasEventos)
                .Include(e => e.Pedidos)
                .FirstOrDefaultAsync(e => e.Id == eventoId.Value);

            if (evento == null)
                return "❌ Evento não encontrado.";

            var diasRestantes = (evento.DataEvento - DateTime.Now).Days;
            var tarefasConcluidas = evento.TarefasEventos.Count(t => t.StatusConclusao == "Concluída");
            var tarefasTotal = evento.TarefasEventos.Count;
            var convidadosConfirmados = evento.ListasConvidados.Count(c => c.ConfirmaPresenca == "Confirmado");
            var totalConvidados = evento.ListasConvidados.Count;
            var despesasTotal = evento.Despesas.Sum(d => d.Valor);
            var percentualOrcamento = evento.CustoEstimado > 0 ? (despesasTotal / evento.CustoEstimado) * 100 : 0;

            var status = diasRestantes < 7 ? "🔥 URGENTE" : diasRestantes < 30 ? "⚠️ ATENÇÃO" : "✅ NO PRAZO";

            return $"## 📊 Progresso do Evento: **{evento.NomeEvento}**\n\n" +
                   $"### 📅 Timeline\n" +
                   $"- **Status:** {status}\n" +
                   $"- **Dias restantes:** {diasRestantes} dias\n" +
                   $"- **Data:** {evento.DataEvento:dd/MM/yyyy}\n\n" +
                   $"### ✅ Conclusão de Tarefas\n" +
                   $"- **Concluídas:** {tarefasConcluidas} de {tarefasTotal} ({(tarefasTotal > 0 ? (tarefasConcluidas * 100.0 / tarefasTotal):0):F0}%)\n" +
                   $"- **Pendentes:** {tarefasTotal - tarefasConcluidas}\n\n" +
                   $"### 👥 Convidados\n" +
                   $"- **Confirmados:** {convidadosConfirmados} de {totalConvidados} ({(totalConvidados > 0 ? (convidadosConfirmados * 100.0 / totalConvidados):0):F0}%)\n\n" +
                   $"### 💰 Orçamento\n" +
                   $"- **Gasto:** R$ {despesasTotal:F2} de R$ {evento.CustoEstimado:F2} ({percentualOrcamento:F0}%)\n" +
                   $"- **Restante:** R$ {(evento.CustoEstimado - despesasTotal):F2}\n\n" +
                   $"### 🎯 **Próximos Passos Recomendados:**\n" +
                   (tarefasTotal - tarefasConcluidas > 0 ? $"- Focar nas {tarefasTotal - tarefasConcluidas} tarefas pendentes\n" : "") +
                   (convidadosConfirmados < totalConvidados * 0.7 ? $"- Fazer follow-up dos convites (apenas {(convidadosConfirmados * 100.0 / totalConvidados):F0}% confirmaram)\n" : "") +
                   (percentualOrcamento > 80 ? $"- ⚠️ Atenção ao orçamento (já usado {percentualOrcamento:F0}%)\n" : "");
        }

        private async Task<string> AnalisarConvidados(int? eventoId)
        {
            if (!eventoId.HasValue)
            {
                return "Por favor, selecione um evento específico para analisar os convidados! 🎯";
            }

            var evento = await _context.Eventos
                .Include(e => e.ListasConvidados)
                .FirstOrDefaultAsync(e => e.Id == eventoId.Value);

            if (evento == null)
                return "❌ Evento não encontrado.";

            var confirmados = evento.ListasConvidados.Count(c => c.ConfirmaPresenca == "Confirmado");
            var pendentes = evento.ListasConvidados.Count(c => c.ConfirmaPresenca == "Pendente");
            var rejeitados = evento.ListasConvidados.Count(c => c.ConfirmaPresenca == "Rejeitado");
            var total = evento.ListasConvidados.Count;

            if (total == 0)
            {
                return $"## 👥 Convidados do Evento: **{evento.NomeEvento}**\n\n" +
                       $"📝 Ainda não há convidados cadastrados para este evento.\n\n" +
                       $"💡 **Sugestão:** Comece adicionando sua lista de convidados!";
            }

            var percentualConfirmacao = total > 0 ? (confirmados * 100.0 / total) : 0;
            var statusGeral = percentualConfirmacao >= 70 ? "✅ EXCELENTE" : 
                             percentualConfirmacao >= 50 ? "⚠️ BOM" : "🔴 ATENÇÃO";

            return $"## 👥 Status dos Convidados: **{evento.NomeEvento}**\n\n" +
                   $"### 📊 Resumo Geral\n" +
                   $"- **Status:** {statusGeral} ({percentualConfirmacao:F0}% de confirmação)\n" +
                   $"- **Total de convidados:** {total}\n\n" +
                   $"### 📈 Detalhamento\n" +
                   $"- ✅ **Confirmados:** {confirmados} ({(total > 0 ? confirmados * 100.0 / total : 0):F0}%)\n" +
                   $"- ⏳ **Pendentes:** {pendentes} ({(total > 0 ? pendentes * 100.0 / total : 0):F0}%)\n" +
                   $"- ❌ **Rejeitaram:** {rejeitados} ({(total > 0 ? rejeitados * 100.0 / total : 0):F0}%)\n\n" +
                   $"### 🎯 **Recomendações:**\n" +
                   (pendentes > 0 ? $"- 📞 Fazer follow-up com {pendentes} convidados pendentes\n" : "") +
                   (percentualConfirmacao < 70 ? $"- 📧 Enviar lembretes personalizados\n" : "") +
                   (confirmados > evento.PublicoEstimado ? $"- ⚠️ Confirmações ({confirmados}) excedem público estimado ({evento.PublicoEstimado})\n" : "") +
                   $"- 📋 Planejar para {Math.Max(confirmados, evento.PublicoEstimado)} pessoas";
        }

        private async Task<string> AnalisarTarefas(int? eventoId)
        {
            if (!eventoId.HasValue)
            {
                return "Por favor, selecione um evento específico para analisar as tarefas! 🎯";
            }

            var evento = await _context.Eventos
                .Include(e => e.TarefasEventos)
                .FirstOrDefaultAsync(e => e.Id == eventoId.Value);

            if (evento == null)
                return "❌ Evento não encontrado.";

            var concluidas = evento.TarefasEventos.Count(t => t.StatusConclusao == "Concluída");
            var pendentes = evento.TarefasEventos.Count(t => t.StatusConclusao == "Pendente");
            var total = evento.TarefasEventos.Count;

            if (total == 0)
            {
                return $"## ✅ Tarefas do Evento: **{evento.NomeEvento}**\n\n" +
                       $"📝 Ainda não há tarefas cadastradas para este evento.\n\n" +
                       $"💡 **Sugestão:** Crie um checklist das principais atividades!";
            }

            var percentualConclusao = total > 0 ? (concluidas * 100.0 / total) : 0;
            var diasRestantes = (evento.DataEvento - DateTime.Now).Days;

            var statusTarefas = percentualConclusao >= 80 ? "✅ EXCELENTE" :
                               percentualConclusao >= 60 ? "⚠️ BOM" : "🔴 ATENÇÃO";

            var tarefasPendentesLista = evento.TarefasEventos
                .Where(t => t.StatusConclusao == "Pendente")
                .Take(5)
                .Select(t => $"- 📌 {t.DescricaoTarefaEvento}")
                .ToList();

            return $"## ✅ Status das Tarefas: **{evento.NomeEvento}**\n\n" +
                   $"### 📊 Resumo Geral\n" +
                   $"- **Status:** {statusTarefas} ({percentualConclusao:F0}% concluído)\n" +
                   $"- **Dias restantes:** {diasRestantes}\n\n" +
                   $"### 📈 Progresso\n" +
                   $"- ✅ **Concluídas:** {concluidas} tarefas\n" +
                   $"- ⏳ **Pendentes:** {pendentes} tarefas\n" +
                   $"- 📊 **Total:** {total} tarefas\n\n" +
                   (tarefasPendentesLista.Any() ? 
                       $"### 📋 **Próximas Tarefas Pendentes:**\n{string.Join("\n", tarefasPendentesLista)}\n\n" : "") +
                   $"### 🎯 **Recomendações:**\n" +
                   (pendentes > 0 && diasRestantes < 14 ? $"- 🔥 **URGENTE:** {pendentes} tarefas com menos de 2 semanas!\n" : "") +
                   (percentualConclusao < 50 && diasRestantes < 30 ? $"- ⚠️ Acelerar ritmo de execução das tarefas\n" : "") +
                   (pendentes == 0 ? $"- 🎉 **Parabéns!** Todas as tarefas concluídas!\n" : $"- 📝 Focar nas {pendentes} tarefas restantes");
        }

        private async Task<string> AnalisarOrcamentoDetalhado(int? eventoId)
        {
            if (!eventoId.HasValue)
            {
                var totalEventos = await _context.Eventos.CountAsync();
                var orcamentoTotalGeral = await _context.Eventos.SumAsync(e => e.CustoEstimado);
                return $"💰 **Resumo Financeiro Geral**\n\n" +
                       $"Você tem **{totalEventos} eventos** com orçamento total de **R$ {orcamentoTotalGeral:F2}**.\n\n" +
                       $"Selecione um evento específico para análise financeira detalhada! 🎯";
            }

            var evento = await _context.Eventos
                .Include(e => e.Despesas)
                .FirstOrDefaultAsync(e => e.Id == eventoId.Value);

            if (evento == null)
                return "❌ Evento não encontrado.";

            var totalGasto = evento.Despesas.Sum(d => d.Valor);
            var orcamentoRestante = evento.CustoEstimado - totalGasto;
            var percentualGasto = evento.CustoEstimado > 0 ? (totalGasto / evento.CustoEstimado) * 100 : 0;
            var diasRestantes = (evento.DataEvento - DateTime.Now).Days;

            var statusFinanceiro = percentualGasto <= 70 ? "✅ SAUDÁVEL" :
                                  percentualGasto <= 90 ? "⚠️ ATENÇÃO" : "🔴 CRÍTICO";

            // Categorizar despesas por descrição (já que não há campo Categoria)
            var categorias = evento.Despesas
                .GroupBy(d => d.Descricao.Length > 20 ? d.Descricao.Substring(0, 20) + "..." : d.Descricao)
                .Select(g => new { Categoria = g.Key, Total = g.Sum(d => d.Valor) })
                .OrderByDescending(c => c.Total)
                .Take(5)
                .ToList();

            var categoriasMaiorGasto = string.Join("\n", categorias.Select(c => 
                $"- **{c.Categoria}:** R$ {c.Total:F2} ({(totalGasto > 0 ? c.Total / totalGasto * 100 : 0):F0}%)"));

            return $"## 💰 Análise Financeira: **{evento.NomeEvento}**\n\n" +
                   $"### 📊 Status Atual\n" +
                   $"- **Situação:** {statusFinanceiro}\n" +
                   $"- **Orçamento total:** R$ {evento.CustoEstimado:F2}\n" +
                   $"- **Gasto atual:** R$ {totalGasto:F2} ({percentualGasto:F0}%)\n" +
                   $"- **Disponível:** R$ {orcamentoRestante:F2}\n" +
                   $"- **Dias restantes:** {diasRestantes}\n\n" +
                   $"### 💸 **Maiores Gastos:**\n" +
                   (categorias.Any() ? categoriasMaiorGasto : "Nenhuma despesa cadastrada ainda") + "\n\n" +
                   $"### 🎯 **Recomendações:**\n" +
                   (percentualGasto > 90 ? $"- 🚨 **ALERTA:** Orçamento quase esgotado!\n" : "") +
                   (orcamentoRestante < 0 ? $"- 💸 **EXCESSO:** R$ {Math.Abs(orcamentoRestante):F2} acima do orçamento!\n" : "") +
                   (diasRestantes > 0 && orcamentoRestante > 0 ? $"- 💡 Você pode gastar R$ {(orcamentoRestante / Math.Max(diasRestantes, 1)):F2} por dia até o evento\n" : "") +
                   (percentualGasto < 50 && diasRestantes < 30 ? $"- ✅ Ótimo controle! Ainda há R$ {orcamentoRestante:F2} disponíveis\n" : "") +
                   $"- 📊 Monitore os gastos principais regularmente";
        }

        private async Task<string> AnalisarFornecedores(int? eventoId)
        {
            var fornecedores = await _context.Fornecedores
                .Include(f => f.Pessoa)
                .Include(f => f.Produtos)
                .ToListAsync();

            if (!fornecedores.Any())
            {
                return $"## 🏪 Análise de Fornecedores\n\n" +
                       $"📝 Ainda não há fornecedores cadastrados no sistema.\n\n" +
                       $"💡 **Sugestão:** Cadastre fornecedores para receber recomendações personalizadas!";
            }

            var melhoresAvaliados = fornecedores
                .Where(f => f.AvaliacaoMedia > 0)
                .OrderByDescending(f => f.AvaliacaoMedia)
                .Take(5)
                .ToList();

            var maisEconomicos = fornecedores
                .Where(f => f.Produtos.Any())
                .OrderBy(f => f.Produtos.Average(p => p.Preco))
                .Take(3)
                .ToList();

            var resultado = new StringBuilder();
            resultado.AppendLine($"## 🏪 Análise de Fornecedores Disponíveis\n");
            resultado.AppendLine($"### 📊 Resumo Geral");
            resultado.AppendLine($"- **Total de fornecedores:** {fornecedores.Count}");
            resultado.AppendLine($"- **Com avaliação:** {fornecedores.Count(f => f.AvaliacaoMedia > 0)}");
            resultado.AppendLine($"- **Com produtos:** {fornecedores.Count(f => f.Produtos.Any())}\n");

            if (melhoresAvaliados.Any())
            {
                resultado.AppendLine($"### ⭐ **Fornecedores Mais Bem Avaliados:**");
                foreach (var fornecedor in melhoresAvaliados)
                {
                    resultado.AppendLine($"- **{fornecedor.Pessoa.Nome}** - ⭐ {fornecedor.AvaliacaoMedia:F1} ({fornecedor.Produtos.Count} produtos)");
                }
                resultado.AppendLine();
            }

            if (maisEconomicos.Any())
            {
                resultado.AppendLine($"### 💰 **Opções Mais Econômicas:**");
                foreach (var fornecedor in maisEconomicos)
                {
                    var precoMedio = fornecedor.Produtos.Any() ? fornecedor.Produtos.Average(p => p.Preco) : 0;
                    resultado.AppendLine($"- **{fornecedor.Pessoa.Nome}** - Média R$ {precoMedio:F2}");
                }
                resultado.AppendLine();
            }

            resultado.AppendLine($"### 🎯 **Recomendações:**");
            resultado.AppendLine($"- 🔍 Compare preços e avaliações antes de contratar");
            resultado.AppendLine($"- 📞 Entre em contato com os fornecedores bem avaliados");
            resultado.AppendLine($"- 💡 Considere contratar fornecedores com histórico positivo");

            return resultado.ToString();
        }

        private async Task<string> DarDicasEconomia(int? eventoId)
        {
            var evento = eventoId.HasValue ? await _context.Eventos
                .Include(e => e.Despesas)
                .FirstOrDefaultAsync(e => e.Id == eventoId.Value) : null;

            var resultado = new StringBuilder();
            resultado.AppendLine($"## 💡 Dicas de Economia para Eventos\n");

            if (evento != null)
            {
                var gastoAtual = evento.Despesas.Sum(d => d.Valor);
                var percentualGasto = evento.CustoEstimado > 0 ? (gastoAtual / evento.CustoEstimado) * 100 : 0;
                
                resultado.AppendLine($"### 📊 Situação Atual: **{evento.NomeEvento}**");
                resultado.AppendLine($"- Gasto atual: R$ {gastoAtual:F2} ({percentualGasto:F0}% do orçamento)");
                resultado.AppendLine($"- Restante: R$ {(evento.CustoEstimado - gastoAtual):F2}\n");
            }

            resultado.AppendLine($"### 🎯 **Estratégias de Economia:**");
            resultado.AppendLine($"- 🤝 **Negocie em lotes:** Contrate vários serviços do mesmo fornecedor");
            resultado.AppendLine($"- 📅 **Evite datas populares:** Sextas, sábados e feriados são mais caros");
            resultado.AppendLine($"- ⏰ **Planeje com antecedência:** Reservas antecipadas têm melhores preços");
            resultado.AppendLine($"- 🏠 **Considere locais alternativos:** Espaços não tradicionais podem ser econômicos");
            resultado.AppendLine($"- 👥 **Aproveite parcerias:** Fornecedores parceiros oferecem descontos");
            resultado.AppendLine($"- 🍽️ **Buffet vs À la carte:** Compare opções de catering");
            resultado.AppendLine($"- 🎵 **Som próprio:** Use equipamentos próprios ou alugue diretamente");
            resultado.AppendLine($"- 🌸 **Decoração sazonal:** Use elementos da estação do ano\n");

            resultado.AppendLine($"### 💰 **Dicas de Orçamento:**");
            resultado.AppendLine($"- 📊 Reserve 10-15% para imprevistos");
            resultado.AppendLine($"- 🎯 Priorize gastos: Local > Comida > Decoração > Extras");
            resultado.AppendLine($"- 📋 Compare pelo menos 3 orçamentos para cada serviço");
            resultado.AppendLine($"- 💳 Negocie formas de pagamento (à vista = desconto)");

            return resultado.ToString();
        }

        private async Task<string> RespostaGenerica(int? eventoId)
        {
            if (eventoId.HasValue)
            {
                var evento = await _context.Eventos.FindAsync(eventoId.Value);
                if (evento != null)
                {
                    return $"## 🤖 Como posso ajudar com: **{evento.NomeEvento}**?\n\n" +
                           $"Estou aqui para auxiliar na organização do seu evento! Posso ajudar com:\n\n" +
                           $"- 📊 **Análise de progresso** e status geral\n" +
                           $"- 👥 **Gestão de convidados** e confirmações\n" +
                           $"- 💰 **Controle de orçamento** e despesas\n" +
                           $"- ✅ **Acompanhamento de tarefas** pendentes\n" +
                           $"- 🏪 **Sugestões de fornecedores** e produtos\n" +
                           $"- 💡 **Dicas de economia** e otimização\n\n" +
                           $"**Pergunte algo específico** ou use os botões de ação rápida!";
                }
            }

            return $"## 🤖 Assistente Virtual EventX\n\n" +
                   $"Olá! Sou seu assistente virtual especializado em gestão de eventos.\n\n" +
                   $"**Como posso ajudá-lo hoje?**\n\n" +
                   $"- 📋 **Selecione um evento** na barra lateral para análises específicas\n" +
                   $"- 💬 **Faça perguntas** sobre progresso, orçamento, convidados...\n" +
                   $"- 🎯 **Use as ações rápidas** para análises instantâneas\n" +
                   $"- 💡 **Experimente as perguntas sugeridas** abaixo\n\n" +
                   $"Estou aqui para tornar seu evento um sucesso! 🎉";
        }

        private async Task<string> CriarOrcamentoInteligente(int? eventoId, string pergunta)
        {
            try
            {
                if (!eventoId.HasValue)
                {
                    return "Para criar um orçamento personalizado, selecione um evento específico primeiro.";
                }

                var evento = await _context.Eventos.FindAsync(eventoId.Value);
                if (evento == null)
                {
                    return "Evento não encontrado.";
                }

                var fornecedores = await _context.Fornecedores
                    .Include(f => f.Produtos)
                    .Include(f => f.Pessoa)
                    .ToListAsync();

                if (!fornecedores.Any())
                {
                    return GerarOrcamentoBasico(evento);
                }

                return GerarOrcamentoComFornecedores(evento, fornecedores);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erro ao criar orçamento: {ex.Message}");
                return $"❌ Erro ao criar orçamento personalizado. Vou gerar sugestões básicas baseadas no seu evento.";
            }
        }

        private string GerarOrcamentoBasico(Evento evento)
        {
            var orcamentoPorPessoa = evento.CustoEstimado / Math.Max(evento.PublicoEstimado, 1);
            
            return $"## 💰 Orçamento Sugerido: **{evento.NomeEvento}**\n\n" +
                   $"### 📊 Informações Básicas\n" +
                   $"- **Tipo:** {evento.TipoEvento}\n" +
                   $"- **Público:** {evento.PublicoEstimado} pessoas\n" +
                   $"- **Orçamento total:** R$ {evento.CustoEstimado:F2}\n" +
                   $"- **Por pessoa:** R$ {orcamentoPorPessoa:F2}\n\n" +
                   $"### 🎯 **Distribuição Sugerida do Orçamento:**\n" +
                   $"- 🏠 **Local (40%):** R$ {(evento.CustoEstimado * 0.4m):F2}\n" +
                   $"- 🍽️ **Alimentação (30%):** R$ {(evento.CustoEstimado * 0.3m):F2}\n" +
                   $"- 🎨 **Decoração (15%):** R$ {(evento.CustoEstimado * 0.15m):F2}\n" +
                   $"- 🎵 **Entretenimento (10%):** R$ {(evento.CustoEstimado * 0.1m):F2}\n" +
                   $"- 🛡️ **Reserva (5%):** R$ {(evento.CustoEstimado * 0.05m):F2}\n\n" +
                   $"### 💡 **Próximos Passos:**\n" +
                   $"1. 🏪 Cadastre fornecedores no sistema para orçamentos detalhados\n" +
                   $"2. 📋 Compare preços de diferentes prestadores\n" +
                   $"3. 🤝 Negocie pacotes e descontos\n" +
                   $"4. 📊 Monitore gastos conforme contrata os serviços";
        }

        private string GerarOrcamentoComFornecedores(Evento evento, List<Fornecedor> fornecedores)
        {
            var resultado = new StringBuilder();
            resultado.AppendLine($"## 💰 Orçamento Inteligente: **{evento.NomeEvento}**\n");

            var orcamentoPorCategoria = new Dictionary<string, decimal>
            {
                ["Local"] = evento.CustoEstimado * 0.4m,
                ["Alimentação"] = evento.CustoEstimado * 0.3m,
                ["Decoração"] = evento.CustoEstimado * 0.15m,
                ["Som"] = evento.CustoEstimado * 0.1m,
                ["Outros"] = evento.CustoEstimado * 0.05m
            };

            resultado.AppendLine($"### 📊 Resumo do Orçamento");
            resultado.AppendLine($"- **Orçamento Total:** R$ {evento.CustoEstimado:F2}");
            resultado.AppendLine($"- **Público Estimado:** {evento.PublicoEstimado} pessoas");
            resultado.AppendLine($"- **Por pessoa:** R$ {evento.CustoEstimado / Math.Max(evento.PublicoEstimado, 1):F2}\n");

            resultado.AppendLine($"### 🛍️ **Fornecedores Recomendados:**");

            foreach (var categoria in orcamentoPorCategoria)
            {
                var fornecedoresDaCategoria = fornecedores
                    .Where(f => f.Produtos.Any(p => p.Tipo.ToLower().Contains(categoria.Key.ToLower())))
                    .OrderByDescending(f => f.AvaliacaoMedia)
                    .Take(2)
                    .ToList();

                if (fornecedoresDaCategoria.Any())
                {
                    resultado.AppendLine($"\n**{categoria.Key}** - Orçamento: R$ {categoria.Value:F2}");
                    foreach (var fornecedor in fornecedoresDaCategoria)
                    {
                        var produtosDaCategoria = fornecedor.Produtos
                            .Where(p => p.Tipo.ToLower().Contains(categoria.Key.ToLower()))
                            .OrderBy(p => p.Preco)
                            .Take(2);

                        resultado.AppendLine($"- **{fornecedor.Pessoa.Nome}** (⭐ {fornecedor.AvaliacaoMedia:F1})");
                        foreach (var produto in produtosDaCategoria)
                        {
                            resultado.AppendLine($"  • {produto.Nome} - R$ {produto.Preco:F2}");
                        }
                    }
                }
            }

            resultado.AppendLine($"\n### 🎯 **Recomendações:**");
            resultado.AppendLine($"- 📞 Entre em contato com os fornecedores listados");
            resultado.AppendLine($"- 💰 Negocie pacotes combinados para economizar");
            resultado.AppendLine($"- ⭐ Priorize fornecedores com melhor avaliação");
            resultado.AppendLine($"- 📋 Solicite orçamentos detalhados antes de fechar");

            return resultado.ToString();
        }

        public async Task<string> GerarSugestaoEventoAsync(int eventoId)
        {
            return await AnalisarProgressoEvento(eventoId);
        }

        public async Task<string> AnalisarOrcamentoAsync(int eventoId)
        {
            return await AnalisarOrcamentoDetalhado(eventoId);
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
                            AlgoritmoIA = "Fallback Inteligente + Dados do Sistema",
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
                Console.WriteLine($"⚠️ Erro ao salvar interação: {ex.Message}");
            }
        }
    }
}
