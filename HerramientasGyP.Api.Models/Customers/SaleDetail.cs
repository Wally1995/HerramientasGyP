using System.ComponentModel.DataAnnotations;
using HerramientasGyP.Api.Models.Inventories;

namespace HerramientasGyP.Api.Models.Customers;

public class SaleDetail
{
    public Guid Id { get; set; }

    [Required]
    public Guid SaleId { get; set; }
    public Sale Sale { get; set; }
    
    [Required]
    public string ProductId { get; set; }
    public Product Product { get; set; }

    [Required]
    public Guid WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; }

    [Required]
    public int Quantity { get; set; }
    
    [Required]
    public decimal UnitPrice { get; set; }
    
    [Required]
    public decimal UnitCost { get; set; } // Cost at sale time — used for profit/margin
    
    [Required]
    public decimal Total => Quantity * UnitPrice; 
}