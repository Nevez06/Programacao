using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ProjetoEventX.ViewModels.Social
{
    public class CriarPostViewModel
    {
        [StringLength(120)]
        public string? Titulo { get; set; }

        [Required(ErrorMessage = "a legenda é obrigatória")]
        [StringLength(2000)]
        public string Legenda { get; set; } = string.Empty;

        [StringLength(80)]
        public string? Categoria { get; set; }

        [StringLength(80)]
        public string? TipoConteudo { get; set; }

        [StringLength(140)]
        public string? Localizacao { get; set; }

        public int? EventoId { get; set; }

        [Required(ErrorMessage = "a imagem é obrigatória")]
        public IFormFile? Imagem { get; set; }

        public List<SelectListItem> EventosDisponiveis { get; set; } = new();
    }
}
