using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoEventX.Data; // Seu namespace do DbContext
using ProjetoEventX.Services;
using ProjetoEventX.DTOs;

namespace ProjetoEventX.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AssistenteController : ControllerBase
    {
        private readonly EventXContext _context;
        private readonly GeminiEventService _geminiService;

        public AssistenteController(EventXContext context, GeminiEventService geminiService)
        {
            _context = context;
            _geminiService = geminiService;
        }

        [HttpPost("planejar")]
        public async Task<IActionResult> PlanejarEvento([FromBody] PedidoRequest pedido)
        {
            // 1. BUSCAR NO BANCO DE DADOS
            // Filtramos fornecedores pela cidade do cliente para a IA não sugerir algo de outro estado.

            var itensDaRegiao = await _context.Fornecedores
                .Include(f => f.Pessoa)   // Para pegar o Nome
                .Include(f => f.Produtos) // Para pegar os Preços
                .Where(f => f.Pessoa.Cidade == pedido.CidadeUsuario) // FILTRO DE REGIÃO
                .SelectMany(f => f.Produtos.Select(p => new ItemParaIA
                {
                    // Achatamos os dados aqui: Fornecedor + Produto viram um item só
                    FornecedorNome = f.Pessoa.Nome,
                    Categoria = f.TipoServico,
                    NotaFornecedor = f.AvaliacaoMedia,
                    NomeProduto = p.Nome, // Assumindo que Produto tem Nome
                    Preco = p.Preco       // Assumindo que Produto tem Preço
                }))
                .ToListAsync();

            if (!itensDaRegiao.Any())
            {
                return Ok(new { resposta = $"Não encontrei fornecedores cadastrados em {pedido.CidadeUsuario}." });
            }

            // 2. ENVIAR PARA A IA
            var plano = await _geminiService.CriarOrcamento(
                itensDaRegiao,
                pedido.DescricaoEvento,
                pedido.Orcamento,
                pedido.Detalhes
            );

            return Ok(new { resposta = plano });
        }
    }

    // Classe simples para receber os dados do Front-end (React/HTML)
    public class PedidoRequest
    {
        public string CidadeUsuario { get; set; }
        public string DescricaoEvento { get; set; } // ex: "Aniversário Infantil"
        public decimal Orcamento { get; set; }
        public string Detalhes { get; set; }
    }
}