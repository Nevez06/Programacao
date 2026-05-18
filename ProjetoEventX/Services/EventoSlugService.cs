using Microsoft.EntityFrameworkCore;
using ProjetoEventX.Data;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace ProjetoEventX.Services
{
    public class EventoSlugService
    {
        private readonly EventXContext _context;

        public EventoSlugService(EventXContext context)
        {
            _context = context;
        }

        public async Task<string> GerarSlugUnicoAsync(string nome, int? eventoIdExcluir = null)
        {
            var slug = GerarSlug(nome);
            var slugBase = slug;
            var contador = 1;

            while (await _context.Eventos.AnyAsync(e => e.Slug == slug && (eventoIdExcluir == null || e.Id != eventoIdExcluir.Value)))
            {
                slug = $"{slugBase}-{contador}";
                contador++;
            }

            return slug;
        }

        private static string GerarSlug(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
            {
                return Guid.NewGuid().ToString("N");
            }

            var normalizado = texto.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var c in normalizado)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }

            var semAcentos = sb.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
            var slug = Regex.Replace(semAcentos, @"[^a-z0-9\s-]", "");
            slug = Regex.Replace(slug, @"[\s-]+", "-").Trim('-');
            return slug.Length > 280 ? slug.Substring(0, 280) : slug;
        }
    }
}
