using System.ComponentModel.DataAnnotations;

namespace EventX.Api.Entities;

public sealed class SupplierCategory
{
    public int Id { get; set; }

    [Required]
    [MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public int SortOrder { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Supplier> Suppliers { get; set; } = new List<Supplier>();
}
