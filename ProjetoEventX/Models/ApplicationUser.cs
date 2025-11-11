using Microsoft.AspNetCore.Identity;

namespace ProjetoEventX.Models
{
    public class ApplicationUser : IdentityUser<int>
    {
        public string? TipoUsuario { get; set; } // "Organizador", "Fornecedor", "Convidado"
    }
}


