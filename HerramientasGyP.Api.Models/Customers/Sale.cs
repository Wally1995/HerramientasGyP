using System.ComponentModel.DataAnnotations;

namespace HerramientasGyP.Api.Models.Customers;

public class Sale
{
    public Guid Id { get; set; }
    
    [Required]
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; }
    
    public string? CustomerGenericName { get; set; }
    public string? CustomerGenericIdNo { get; set; }
    
    [Required]
    public DateTime SaleDate { get; set; }
    
    [Required]
    public DateTime DueDate { get; set; }
    
    [Required]
    public decimal TotalAmount { get; set; }
    
    [Required]
    public PaymentMethod PaymentMethod { get; set; }

    [Required] 
    public bool Status { get; set; } = true;

    [Required]
    public bool PrintedStatus { get; set; } = false;
    
    public string? DeliveryAddress { get; set; }
    public DateTime? DeliveryDate { get; set; }

    public ICollection<SaleDetail> SaleDetails { get; set; }
}