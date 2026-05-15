using System.ComponentModel.DataAnnotations;

namespace ProjetoEventX.DTOs.Quotes
{
    public class QuoteDto
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public int SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public int OrganizerId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal EstimatedValue { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string? ResponseMessage { get; set; }
        public decimal? ResponseValue { get; set; }
        public DateTime? ResponseDate { get; set; }
        public decimal? CounterProposalValue { get; set; }
        public string? CounterProposalMessage { get; set; }
        public DateTime? CounterProposalDate { get; set; }
        public int CurrentRound { get; set; }
        public DateTime? ExpireAt { get; set; }
        public Guid? GeneratedOrderId { get; set; }
    }

    public class CreateQuoteDto
    {
        [Required]
        public int EventId { get; set; }

        [Required]
        public int SupplierId { get; set; }

        [Required]
        [StringLength(200, MinimumLength = 3)]
        public string ServiceName { get; set; } = string.Empty;

        [Required]
        [StringLength(2000, MinimumLength = 3)]
        public string Description { get; set; } = string.Empty;

        [Range(0.01, 10000000)]
        public decimal EstimatedValue { get; set; }

        public DateTime? ExpireAt { get; set; }
    }

    public class UpdateQuoteStatusDto
    {
        [Required]
        [StringLength(30)]
        public string Status { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Message { get; set; }

        [Range(0.01, 10000000)]
        public decimal? Value { get; set; }
    }

    public class NegotiateQuoteDto
    {
        [Required]
        [StringLength(40)]
        public string Action { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Message { get; set; }

        [Range(0.01, 10000000)]
        public decimal? Value { get; set; }
    }
}