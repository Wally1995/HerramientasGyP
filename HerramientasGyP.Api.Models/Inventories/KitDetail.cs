using System.ComponentModel.DataAnnotations;

namespace HerramientasGyP.Api.Models.Inventories;

public class KitDetail
{
    [Key]
    [MaxLength(10)]
    public string KitId { get; set; }
    public Kit Kit { get; set; }

    [Required]
    public string ProductId { get; set; }
    public Product Product { get; set; }

    [Required]
    public int Quantity { get; set; }

    [Required]
    public decimal Price { get; set; } // Price of this item *within* the kit
}