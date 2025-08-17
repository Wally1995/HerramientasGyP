using System.ComponentModel.DataAnnotations;

namespace HerramientasGyP.Api.Models.Inventories;

public class ProductWarehouse
{
    [Required]
    public string ProductId { get; set; }
    public Product Product { get; set; }

    [Required]
    public Guid WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; }

    [Required]
    public int Quantity { get; set; } // How many units of this product are in this warehouse
}