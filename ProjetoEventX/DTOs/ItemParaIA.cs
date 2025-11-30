namespace ProjetoEventX.DTOs
{
    // Essa classe serve apenas para mandar dados para o Gemini
    public class ItemParaIA
    {
        public string FornecedorNome { get; set; }
        public string Categoria { get; set; }
        public decimal NotaFornecedor { get; set; }
        public string NomeProduto { get; set; }
        public decimal Preco { get; set; }
    }
}