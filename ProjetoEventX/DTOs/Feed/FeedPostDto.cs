namespace ProjetoEventX.DTOs.Feed
{
    public class FeedPostDto
    {
        public int Id { get; set; }
        public int AutorUserId { get; set; }
        public string AutorNome { get; set; } = string.Empty;
        public string? AutorFotoUrl { get; set; }
        public string? Titulo { get; set; }
        public string Conteudo { get; set; } = string.Empty;
        public string ImagemUrl { get; set; } = string.Empty;
        public string? Categoria { get; set; }
        public string? TipoConteudo { get; set; }
        public string? Localizacao { get; set; }
        public DateTime DataCriacao { get; set; }
        public DateTime DataAtualizacao { get; set; }
        public int TotalCurtidas { get; set; }
        public int TotalComentarios { get; set; }
        public bool UsuarioCurtiu { get; set; }
        public bool IsOwner { get; set; }
        public bool CommentsEnabled { get; set; } = true;
        public bool IsPinned { get; set; }
        public bool IsArchived { get; set; }
        public bool HideLikesCount { get; set; }
        public bool HideSharesCount { get; set; }
        public int? EventoId { get; set; }
        public string? NomeEvento { get; set; }
        public List<PostCommentDto> Comentarios { get; set; } = new();
    }
}
