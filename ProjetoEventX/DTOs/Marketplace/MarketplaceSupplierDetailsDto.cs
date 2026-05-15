namespace ProjetoEventX.DTOs.Marketplace
{
    public class MarketplaceSupplierDetailsDto : MarketplaceSupplierDto
    {
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
        public List<string> Services { get; set; } = new();
        public List<string> Gallery { get; set; } = new();
    }
}