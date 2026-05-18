namespace ProjetoEventX.DTOs.Users
{
    public class UserProfileDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FotoUrl { get; set; }
        public string TipoUsuario { get; set; } = string.Empty;
        public string Telefone { get; set; } = string.Empty;
        public string Cidade { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string Endereco { get; set; } = string.Empty;
        public string Cpf { get; set; } = string.Empty;
    }
}
