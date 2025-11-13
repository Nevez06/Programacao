using System.ComponentModel.DataAnnotations;

namespace ProjetoEventX.Models
{
    // LOGIN ORGANIZADOR
    public class LoginOrganizadorViewModel
    {
        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Senha é obrigatória")]
        [DataType(DataType.Password)]
        public string Senha { get; set; } = string.Empty;

    }

    // REGISTRO ORGANIZADOR
    public class RegistroOrganizadorViewModel
    {
        [Required(ErrorMessage = "Nome completo é obrigatório")]
        [Display(Name = "Nome Completo")]
        public string NomeCompleto { get; set; } = string.Empty;

        [Required(ErrorMessage = "CPF é obrigatório")]
        [Display(Name = "CPF")]
        [StringLength(14, ErrorMessage = "O CPF deve ter até 14 caracteres")]
        public string Cpf { get; set; } = string.Empty;

        [Required(ErrorMessage = "Endereço é obrigatório")]
        [StringLength(255)]
        public string Endereco { get; set; } = string.Empty;

        [Required(ErrorMessage = "Telefone é obrigatório")]
        [Display(Name = "Telefone")]
        public string Telefone { get; set; } = string.Empty;


        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Senha é obrigatória")]
        [StringLength(100, ErrorMessage = "A senha deve ter pelo menos {2} caracteres", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Senha { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar Senha")]
        [Compare("Senha", ErrorMessage = "As senhas não coincidem")]
        public string ConfirmarSenha { get; set; } = string.Empty;
    }

    // LOGIN FORNECEDOR
    public class LoginFornecedorViewModel
    {
        [Required(ErrorMessage = "CPF é obrigatório")]
        [Display(Name = "CPF")]
        public string Cpf { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nome da loja é obrigatório")]
        [Display(Name = "Nome da Loja")]
        public string NomeLoja { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Senha é obrigatória")]
        [DataType(DataType.Password)]
        public string Senha { get; set; } = string.Empty;
    }

    // REGISTRO FORNECEDOR
    public class RegistroFornecedorViewModel
    {
        [Required(ErrorMessage = "Nome completo é obrigatório")]
        [Display(Name = "Nome Completo")]
        public string NomeCompleto { get; set; } = string.Empty;

        [Required(ErrorMessage = "CPF é obrigatório")]
        [Display(Name = "CPF")]
        [StringLength(14, ErrorMessage = "O CPF deve ter até 14 caracteres")]
        public string Cpf { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nome da loja é obrigatório")]
        [Display(Name = "Nome da Loja")]
        public string NomeLoja { get; set; } = string.Empty;

        [Required(ErrorMessage = "CNPJ é obrigatório")]
        [Display(Name = "CNPJ")]
        [StringLength(18, ErrorMessage = "O CNPJ deve ter até 18 caracteres")]
        public string Cnpj { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tipo de serviço é obrigatório")]
        [Display(Name = "Tipo de Serviço")]
        [StringLength(255)]
        public string TipoServico { get; set; } = string.Empty;

        [Required(ErrorMessage = "Endereço é obrigatório")]
        [StringLength(255)]
        public string Endereco { get; set; } = string.Empty;

        [Required(ErrorMessage = "Telefone é obrigatório")]
        [Display(Name = "Telefone")]
        public string Telefone { get; set; } = string.Empty;


        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Senha é obrigatória")]
        [StringLength(100, ErrorMessage = "A senha deve ter pelo menos {2} caracteres", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Senha { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar Senha")]
        [Compare("Senha", ErrorMessage = "As senhas não coincidem")]
        public string ConfirmarSenha { get; set; } = string.Empty;
    }

    // LOGIN CONVIDADO
    public class LoginConvidadoViewModel
    {
        [Required(ErrorMessage = "Nome do evento é obrigatório")]
        [Display(Name = "Nome do Evento")]
        public string NomeEvento { get; set; } = string.Empty;

        [Required(ErrorMessage = "Senha é obrigatória")]
        [DataType(DataType.Password)]
        public string Senha { get; set; } = string.Empty;
    }
}
