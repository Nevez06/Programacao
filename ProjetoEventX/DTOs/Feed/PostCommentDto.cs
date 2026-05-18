using System.ComponentModel.DataAnnotations;

namespace ProjetoEventX.DTOs.Feed
{
    public class PostCommentDto
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public int UserId { get; set; }
        public string NomeAutor { get; set; } = string.Empty;

        [Required(ErrorMessage = "Texto do comentario e obrigatorio.")]
        [StringLength(600, ErrorMessage = "Comentario muito longo.")]
        public string Texto { get; set; } = string.Empty;

        public DateTime CriadoEm { get; set; }
        public bool IsOwner { get; set; }
    }
}
