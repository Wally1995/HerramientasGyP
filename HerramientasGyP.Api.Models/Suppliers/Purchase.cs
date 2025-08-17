using System.ComponentModel.DataAnnotations;

namespace HerramientasGyP.Api.Models.Suppliers;

public class Purchase
{
    public Guid Id { get; set; }
    
    [Required]
    public Guid SupplierId { get; set; }
    public Supplier Supplier { get; set; }

    [Required]
    public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;
    
    [Required]
    public DateTime DuePaymentDate { get; set; }
    
    [Required]
    public decimal TotalAmount { get; set; }
    
    public PaymentMethod PaymentMethod { get; set; }
    
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<PurchaseDetail> PurchaseDetails { get; set; }
}