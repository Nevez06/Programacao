namespace ProjetoEventX.DTOs.Social
{
    public class SocialProfileDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string NomeExibicao { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string? Bio { get; set; }
        public string? FotoPerfilUrl { get; set; }
        public string TipoPerfil { get; set; } = string.Empty;
        public string? Cidade { get; set; }
        public string? Instagram { get; set; }
        public string? Site { get; set; }
        public int PostsCount { get; set; }
        public int FollowersCount { get; set; }
        public int FollowingCount { get; set; }
        public bool IsFollowing { get; set; }
        public DateTime AtualizadoEm { get; set; }
    }
}