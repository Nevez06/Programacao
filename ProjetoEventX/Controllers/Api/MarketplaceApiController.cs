using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoEventX.Data;
using ProjetoEventX.DTOs.Marketplace;
using ProjetoEventX.Helpers;

namespace ProjetoEventX.Controllers.Api
{
    [ApiController]
    [Authorize]
    [Route("api/marketplace")]
    public class MarketplaceApiController : ControllerBase
    {
        private const string DefaultProfileImage = "/uploads/social/defaults/default-profile.svg";

        private readonly EventXContext _context;

        public MarketplaceApiController(EventXContext context)
        {
            _context = context;
        }

        [HttpGet("suppliers")]
        public async Task<ActionResult<IEnumerable<MarketplaceSupplierDto>>> GetSuppliers(
            [FromQuery] string? search = null,
            [FromQuery] string? category = null,
            [FromQuery] string? city = null,
            [FromQuery] string? state = null,
            [FromQuery] decimal? priceMin = null,
            [FromQuery] decimal? priceMax = null,
            [FromQuery] string? orderBy = null,
            [FromQuery] int take = 120)
        {
            var normalizedTake = Math.Clamp(take, 1, 400);

            var query = _context.Fornecedores
                .AsNoTracking()
                .Include(f => f.Pessoa)
                .Include(f => f.Produtos)
                .Include(f => f.Avaliacoes)
                .Include(f => f.Pedidos)
                .Include(f => f.SolicitacoesRecebidas)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var normalized = search.Trim().ToLower();
                query = query.Where(f =>
                    (f.Pessoa != null && f.Pessoa.Nome.ToLower().Contains(normalized)) ||
                    f.TipoServico.ToLower().Contains(normalized) ||
                    f.Cidade.ToLower().Contains(normalized));
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                var normalized = category.Trim().ToLower();
                query = query.Where(f => f.TipoServico.ToLower().Contains(normalized));
            }

            if (!string.IsNullOrWhiteSpace(city))
            {
                var normalized = city.Trim().ToLower();
                query = query.Where(f => f.Cidade.ToLower().Contains(normalized));
            }

            if (!string.IsNullOrWhiteSpace(state))
            {
                var normalized = state.Trim().ToUpper();
                query = query.Where(f => f.UF.ToUpper() == normalized);
            }

            if (priceMin.HasValue)
            {
                query = query.Where(f => f.Produtos.Any() && f.Produtos.Min(p => p.Preco) >= priceMin.Value);
            }

            if (priceMax.HasValue)
            {
                query = query.Where(f => f.Produtos.Any() && f.Produtos.Min(p => p.Preco) <= priceMax.Value);
            }

            query = (orderBy ?? string.Empty).Trim().ToLower() switch
            {
                "lowest-price" or "menor-preco" => query.OrderBy(f => f.Produtos.Any() ? f.Produtos.Min(p => p.Preco) : decimal.MaxValue),
                "best-rated" or "melhor-avaliado" => query.OrderByDescending(f => f.AvaliacaoMedia),
                "popular" or "mais-popular" => query.OrderByDescending(f => f.Pedidos.Count),
                "fast-response" or "resposta-rapida" => query.OrderByDescending(f => f.SolicitacoesRecebidas.Count(s => s.Status == "Respondido" || s.Status == "Aceito")),
                _ => query.OrderByDescending(f => f.AvaliacaoMedia).ThenBy(f => f.Pessoa.Nome)
            };

            var suppliers = await query.Take(normalizedTake).ToListAsync();
            var rankingMap = await _context.FornecedorRankings
                .AsNoTracking()
                .Where(r => suppliers.Select(s => s.Id).Contains(r.FornecedorId))
                .ToDictionaryAsync(r => r.FornecedorId);

            var payload = suppliers.Select(f => MapSupplier(f, rankingMap.GetValueOrDefault(f.Id))).ToList();
            return Ok(payload);
        }

        [HttpGet("suppliers/{id:int}")]
        public async Task<ActionResult<MarketplaceSupplierDetailsDto>> GetSupplierById(int id)
        {
            var supplier = await _context.Fornecedores
                .AsNoTracking()
                .Include(f => f.Pessoa)
                .Include(f => f.Produtos)
                .Include(f => f.Avaliacoes)
                .Include(f => f.Pedidos)
                .Include(f => f.SolicitacoesRecebidas)
                .Include(f => f.Portfolio)
                .Include(f => f.Servicos)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (supplier == null)
            {
                return NotFound(new { message = "Fornecedor não encontrado." });
            }

            var ranking = await _context.FornecedorRankings.AsNoTracking().FirstOrDefaultAsync(r => r.FornecedorId == id);
            var mapped = MapSupplier(supplier, ranking);

            return Ok(new MarketplaceSupplierDetailsDto
            {
                Id = mapped.Id,
                Name = mapped.Name,
                Category = mapped.Category,
                City = mapped.City,
                State = mapped.State,
                Description = mapped.Description,
                Rating = mapped.Rating,
                ReviewCount = mapped.ReviewCount,
                StartingPrice = mapped.StartingPrice,
                PriceMin = mapped.PriceMin,
                PriceMax = mapped.PriceMax,
                Featured = mapped.Featured,
                Premium = mapped.Premium,
                RankingPosition = mapped.RankingPosition,
                AcceptanceRate = mapped.AcceptanceRate,
                ResponseRate = mapped.ResponseRate,
                CancellationRate = mapped.CancellationRate,
                PunctualityScore = mapped.PunctualityScore,
                PopularityScore = mapped.PopularityScore,
                RecentPerformanceScore = mapped.RecentPerformanceScore,
                TotalHires = mapped.TotalHires,
                ImageUrl = mapped.ImageUrl,
                Badges = mapped.Badges,
                ContactEmail = supplier.Email,
                ContactPhone = supplier.Telefone,
                Services = supplier.Servicos.Select(s => s.Nome).Where(n => !string.IsNullOrWhiteSpace(n)).Distinct().ToList(),
                Gallery = supplier.Portfolio.Select(p => SocialImagemHelper.NormalizePublicImageUrl(p.Url, DefaultProfileImage)).ToList()
            });
        }

        [HttpGet("categories")]
        public async Task<ActionResult<IEnumerable<string>>> GetCategories()
        {
            var categories = await _context.Fornecedores
                .AsNoTracking()
                .Where(f => !string.IsNullOrWhiteSpace(f.TipoServico))
                .Select(f => f.TipoServico)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            return Ok(categories);
        }

        private static MarketplaceSupplierDto MapSupplier(Models.Fornecedor supplier, Models.FornecedorRanking? ranking)
        {
            var reviewCount = supplier.Avaliacoes.Count;
            var averageRating = reviewCount > 0
                ? Math.Round((double)supplier.Avaliacoes.Average(a => a.Nota), 2)
                : (double)supplier.AvaliacaoMedia;

            var minPrice = supplier.Produtos.Any() ? supplier.Produtos.Min(p => p.Preco) : 0m;
            var maxPrice = supplier.Produtos.Any() ? supplier.Produtos.Max(p => p.Preco) : 0m;

            var acceptanceRate = ranking != null ? decimal.ToDouble(ranking.TaxaAceitacao / 100m) : 0.65;
            var responseRate = ranking != null
                ? decimal.ToDouble(Math.Clamp(100m - ranking.TempoMedioRespostaHoras * 2.5m, 0m, 100m) / 100m)
                : 0.6;
            var cancellationRate = ranking != null ? decimal.ToDouble(ranking.TaxaCancelamento / 100m) : 0.1;
            var punctualityScore = ranking != null
                ? decimal.ToDouble(Math.Clamp(100m - ranking.TaxaCancelamento * 1.2m, 0m, 100m) / 100m)
                : 0.7;

            var popularity = supplier.Pedidos.Count;
            var popularityScore = popularity <= 0 ? 0.25 : Math.Clamp(popularity / 100.0, 0.25, 0.95);

            var recentPerformance = ranking != null
                ? decimal.ToDouble(Math.Clamp(ranking.PontuacaoGeral / 100m, 0m, 1m))
                : Math.Clamp((averageRating / 5d) * 0.7 + acceptanceRate * 0.3, 0d, 1d);

            var badges = new List<string>();
            if (ranking?.IsTopFornecedor == true)
            {
                badges.Add("Top fornecedor");
            }

            if (averageRating >= 4.8 && reviewCount >= 20)
            {
                badges.Add("Melhor avaliado");
            }

            if (responseRate >= 0.8)
            {
                badges.Add("Resposta rápida");
            }

            if (popularity >= 30)
            {
                badges.Add("Mais contratado");
            }

            return new MarketplaceSupplierDto
            {
                Id = supplier.Id,
                Name = supplier.Pessoa?.Nome ?? supplier.NomeNegocio ?? supplier.UserName ?? $"Fornecedor {supplier.Id}",
                Category = supplier.TipoServico,
                City = supplier.Cidade,
                State = supplier.UF,
                Description = supplier.Descricao ?? string.Empty,
                Rating = averageRating,
                ReviewCount = reviewCount,
                StartingPrice = minPrice > 0 ? $"R$ {minPrice:N0}" : "Sob consulta",
                PriceMin = minPrice > 0 ? decimal.ToDouble(minPrice) : null,
                PriceMax = maxPrice > 0 ? decimal.ToDouble(maxPrice) : null,
                Featured = ranking?.IsTopFornecedor == true || averageRating >= 4.8,
                Premium = reviewCount >= 15 && averageRating >= 4.6,
                RankingPosition = ranking?.PosicaoRanking,
                AcceptanceRate = acceptanceRate,
                ResponseRate = responseRate,
                CancellationRate = cancellationRate,
                PunctualityScore = punctualityScore,
                PopularityScore = popularityScore,
                RecentPerformanceScore = recentPerformance,
                TotalHires = popularity,
                ImageUrl = SocialImagemHelper.NormalizePublicImageUrl(supplier.FotoPerfilUrl, DefaultProfileImage),
                Badges = badges
            };
        }
    }
}
