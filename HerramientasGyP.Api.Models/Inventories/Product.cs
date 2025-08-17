using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using HerramientasGyP.Api.Models.Customers;
using HerramientasGyP.Api.Models.Suppliers;

namespace HerramientasGyP.Api.Models.Inventories;

public class Product
{
    [Key]
    [MaxLength(10)]
    public string Id { get; set; } // SKU or internal code
    
    [Required]
    [MaxLength(150)]
    public string Description { get; set; }
    
    [Required]
    public int Quantity { get; set; } = 0; //Total product quantity regardless of were it is
    
    [Required]
    public decimal Cost { get; set; } = 0;  // Latest average cost
    
    [Required]
    public decimal Price { get; set; } = 0;// Current sale price
    
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [Required]
    public bool Deleted { get; set; } = false;

    // Computed at runtime: total quantity across all warehouses
    public ICollection<ProductWarehouse> WarehouseLinks { get; set; }
    public ICollection<SaleDetail> SaleDetails { get; set; }
    public ICollection<PurchaseDetail> PurchaseDetails { get; set; }

}