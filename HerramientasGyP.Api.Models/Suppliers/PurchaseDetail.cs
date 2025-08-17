using System.ComponentModel.DataAnnotations;
using HerramientasGyP.Api.Models.Inventories;

namespace HerramientasGyP.Api.Models.Suppliers;

public class PurchaseDetail
{
    public Guid Id { get; set; }
    
    [Required]
    public Guid PurchaseId { get; set; }
    public Purchase Purchase { get; set; }

    [Required]
    public string ProductId { get; set; }
    public Product Product { get; set; }

    [Required]
    public Guid WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; }

    [Required]
    public int Quantity { get; set; }
    
    [Required]
    public decimal UnitCost { get; set; }
}