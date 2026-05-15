namespace ProjetoEventX.DTOs.Ranking
{
    public class SupplierRankingDto
    {
        public int SupplierId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public int Position { get; set; }
        public decimal Score { get; set; }
        public decimal AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public decimal AcceptanceRate { get; set; }
        public decimal ResponseRate { get; set; }
        public decimal CancellationRate { get; set; }
        public decimal PunctualityScore { get; set; }
        public int CompletedOrders { get; set; }
        public int AttendedEvents { get; set; }
        public bool IsTopSupplier { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}