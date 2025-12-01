namespace ProjetoEventX.DTOs
{
    // Essa classe serve apenas para mandar dados para o Gemini
    public class ItemParaIA
    {
        public required string FornecedorNome { get; set; }
        public required string Categoria { get; set; }
        public decimal NotaFornecedor { get; set; }
        public required string NomeProduto { get; set; }
        public decimal Preco { get; set; }
    }
}