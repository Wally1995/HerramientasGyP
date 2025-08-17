using System.ComponentModel.DataAnnotations;

namespace HerramientasGyP.Api.Models.Suppliers;

public class Supplier
{
    public Guid Id { get; set; }
    
    [Required]
    public string Name { get; set; }
    
    [Required]
    public string IdentificationNumber { get; set; }
    
    [Required]
    public string Address { get; set; }
    
    [Required]
    public int CreditDays { get; set; } = 0;

    [Required]
    [MaxLength(8)]
    public string ContactNumber { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Purchase> Purchases { get; set; }
}