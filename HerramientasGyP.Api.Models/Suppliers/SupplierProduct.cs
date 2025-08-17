using System.ComponentModel.DataAnnotations;
using HerramientasGyP.Api.Models.Inventories;

namespace HerramientasGyP.Api.Models.Suppliers;

public class SupplierProduct
{
    public Guid SupplierId { get; set; }
    public Supplier Supplier { get; set; }

    [Required]
    public string ProductId { get; set; }
    public Product Product { get; set; }

    [Required]
    [MaxLength(50)]
    public string SupplierProductCode { get; set; } // Supplier's internal product ID
}