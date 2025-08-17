using System.ComponentModel.DataAnnotations;

namespace HerramientasGyP.Api.Models.Inventories;

public class Warehouse
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; }
    
    [Required]
    public Guid BranchId { get; set; }
    public Branch Branch { get; set; }
    
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public bool isDeleted { get; set; } = false;

    public ICollection<ProductWarehouse> ProductLinks { get; set; }
}