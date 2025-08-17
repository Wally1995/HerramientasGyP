using System.ComponentModel.DataAnnotations;

namespace HerramientasGyP.Api.Models.Inventories;

public class Kit
{
    [Key]
    [MaxLength(10)]
    public string Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string Name { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public decimal TotalPrice { get; set; } // Selling price of the entire kit

    public ICollection<KitDetail> Items { get; set; }
}