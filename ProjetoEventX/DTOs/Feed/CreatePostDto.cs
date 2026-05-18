using System.ComponentModel.DataAnnotations;

namespace ProjetoEventX.DTOs.Feed
{
    public class CreatePostDto
    {
        [StringLength(120, ErrorMessage = "Titulo muito longo.")]
        public string? Titulo { get; set; }

        [Required(ErrorMessage = "Conteudo do post e obrigatorio.")]
        [StringLength(2000, ErrorMessage = "Conteudo muito longo.")]
        public string Conteudo { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "URL da imagem muito longa.")]
        public string? ImagemUrl { get; set; }

        [StringLength(80, ErrorMessage = "Categoria muito longa.")]
        public string? Categoria { get; set; }

        [StringLength(80, ErrorMessage = "Tipo de conteudo muito longo.")]
        public string? TipoConteudo { get; set; }

        [StringLength(140, ErrorMessage = "Localizacao muito longa.")]
        public string? Localizacao { get; set; }

        public int? EventoId { get; set; }
        public bool? CommentsEnabled { get; set; }
    }
}
