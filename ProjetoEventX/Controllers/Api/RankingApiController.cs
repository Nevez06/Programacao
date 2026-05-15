using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoEventX.Data;
using ProjetoEventX.DTOs.Ranking;
using ProjetoEventX.Services;

namespace ProjetoEventX.Controllers.Api
{
    [ApiController]
    [Authorize]
    [Route("api/ranking")]
    public class RankingApiController : ControllerBase
    {
        private readonly EventXContext _context;
        private readonly FornecedorPerformanceService _performanceService;

        public RankingApiController(EventXContext context, FornecedorPerformanceService performanceService)
        {
            _context = context;
            _performanceService = performanceService;
        }

        [HttpGet]
        [HttpGet("suppliers")]
        public async Task<ActionResult<IEnumerable<SupplierRankingDto>>> GetSuppliers(
            [FromQuery] string? category = null,
            [FromQuery] string? city = null,
            [FromQuery] string? state = null,
            [FromQuery] int take = 120)
        {
            await _performanceService.RecalcularTodosAsync();

            var normalizedTake = Math.Clamp(take, 1, 300);
            var query = _context.FornecedorRankings
                .AsNoTracking()
                .Include(r => r.Fornecedor)
                    .ThenInclude(f => f!.Pessoa)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(category))
            {
                var normalized = category.Trim().ToLower();
                query = query.Where(r => r.Fornecedor != null && r.Fornecedor.TipoServico.ToLower().Contains(normalized));
            }

            if (!string.IsNullOrWhiteSpace(city))
            {
                var normalized = city.Trim().ToLower();
                query = query.Where(r => r.Fornecedor != null && r.Fornecedor.Cidade.ToLower().Contains(normalized));
            }

            if (!string.IsNullOrWhiteSpace(state))
            {
                var normalized = state.Trim().ToUpper();
                query = query.Where(r => r.Fornecedor != null && r.Fornecedor.UF.ToUpper() == normalized);
            }

            var rankings = await query
                .OrderBy(r => r.PosicaoRanking <= 0 ? int.MaxValue : r.PosicaoRanking)
                .ThenByDescending(r => r.PontuacaoGeral)
                .Take(normalizedTake)
                .ToListAsync();

            return Ok(rankings.Select(MapRanking));
        }

        [HttpGet("suppliers/top")]
        public async Task<ActionResult<IEnumerable<SupplierRankingDto>>> GetTopSuppliers([FromQuery] int take = 10)
        {
            await _performanceService.RecalcularTodosAsync();
            var normalizedTake = Math.Clamp(take, 1, 50);

            var top = await _context.FornecedorRankings
                .AsNoTracking()
                .Include(r => r.Fornecedor)
                    .ThenInclude(f => f!.Pessoa)
                .OrderBy(r => r.PosicaoRanking <= 0 ? int.MaxValue : r.PosicaoRanking)
                .ThenByDescending(r => r.PontuacaoGeral)
                .Take(normalizedTake)
                .ToListAsync();

            return Ok(top.Select(MapRanking));
        }

        [HttpGet("suppliers/{id:int}")]
        public async Task<ActionResult<SupplierRankingDto>> GetSupplierRankingById(int id)
        {
            await _performanceService.RecalcularAsync(id);

            var ranking = await _context.FornecedorRankings
                .AsNoTracking()
                .Include(r => r.Fornecedor)
                    .ThenInclude(f => f!.Pessoa)
                .FirstOrDefaultAsync(r => r.FornecedorId == id);

            if (ranking == null)
            {
                return NotFound(new { message = "Ranking do fornecedor não encontrado." });
            }

            return Ok(MapRanking(ranking));
        }

        private static SupplierRankingDto MapRanking(Models.FornecedorRanking ranking)
        {
            return new SupplierRankingDto
            {
                SupplierId = ranking.FornecedorId,
                Name = ranking.Fornecedor?.Pessoa?.Nome ?? ranking.Fornecedor?.NomeNegocio ?? $"Fornecedor {ranking.FornecedorId}",
                Category = ranking.Fornecedor?.TipoServico ?? string.Empty,
                City = ranking.Fornecedor?.Cidade ?? string.Empty,
                State = ranking.Fornecedor?.UF ?? string.Empty,
                Position = ranking.PosicaoRanking,
                Score = ranking.PontuacaoGeral,
                AverageRating = ranking.MediaAvaliacoes,
                ReviewCount = ranking.QuantidadeAvaliacoes,
                AcceptanceRate = ranking.TaxaAceitacao,
                ResponseRate = Math.Clamp(100m - ranking.TempoMedioRespostaHoras * 2.5m, 0m, 100m),
                CancellationRate = ranking.TaxaCancelamento,
                PunctualityScore = Math.Clamp(100m - ranking.TaxaCancelamento * 1.2m, 0m, 100m),
                CompletedOrders = ranking.PedidosConcluidos,
                AttendedEvents = ranking.EventosAtendidos,
                IsTopSupplier = ranking.IsTopFornecedor,
                UpdatedAt = ranking.UltimaAtualizacao
            };
        }
    }
}