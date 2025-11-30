using System.Text;
using System.Text.Json;
using ProjetoEventX.DTOs; 

namespace ProjetoEventX.Services
{
    public class GeminiEventService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey = "AIzaSyAk-FanCR-I6PSt1Vv6hvoGxqEhAzq7b6k";

        public GeminiEventService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> CriarOrcamento(List<ItemParaIA> itensDisponiveis, string tipoEvento, decimal orcamento, string detalhes)
        {
            // Transforma a lista de produtos em texto JSON para a IA ler
            var jsonItens = JsonSerializer.Serialize(itensDisponiveis);

            var prompt = $@"
                ATUE COMO: Um organizador de eventos profissional.
                OBJETIVO: O usuário quer fazer um evento: '{tipoEvento}'.
                ORÇAMENTO MÁXIMO: R$ {orcamento}.
                DETALHES EXTRAS: {detalhes}.

                ABAIXO ESTÁ A LISTA DE SERVIÇOS/PRODUTOS DISPONÍVEIS NA REGIÃO (JSON):
                {jsonItens}

                SUA MISSÃO:
                1. Selecione produtos dessa lista para montar o evento.
                2. A soma dos preços NÃO pode passar de R$ {orcamento}.
                3. Tente escolher fornecedores com maior 'NotaFornecedor'.
                4. Se o orçamento for curto, priorize o essencial (Comida/Local).

                RESPOSTA ESPERADA (Formato Markdown):
                ## 🎉 Plano Sugerido
                | Fornecedor | Item | Preço |
                |---|---|---|
                | [Nome] | [Produto] | R$ [Valor] |
                
                **Total:** R$ [Soma]
                **Comentário:** [Explique suas escolhas]
            ";

            // Monta o corpo da requisição para o Google
            var requestBody = new
            {
                contents = new[]
                {
                    new { parts = new[] { new { text = prompt } } }
                }
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            // Envia para o Gemini (Modelo Flash é rápido, Pro é mais inteligente)
            var response = await _httpClient.PostAsync($"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={_apiKey}", jsonContent);

            if (!response.IsSuccessStatusCode) return "Erro ao contatar a IA.";

            // Lê a resposta
            var responseString = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseString);

            try
            {
                return doc.RootElement.GetProperty("candidates")[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString();
            }
            catch
            {
                return "A IA não conseguiu gerar um plano com os dados fornecidos.";
            }
        }
    }
}