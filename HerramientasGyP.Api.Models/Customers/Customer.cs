using System.ComponentModel.DataAnnotations;

namespace HerramientasGyP.Api.Models.Customers;

public class Customer
{
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string FullName { get; set; }
    
    [MaxLength(14)]
    public string IdentificationNumber { get; set; }
    
    [MaxLength(255)]
    public string Address { get; set; }
    
    [MaxLength(8)]
    public string PhoneNumber { get; set; }
    
    [Required]
    public bool HasCredit { get; set; } = false;

    [Required] 
    public int CreditDays { get; set; } = 0;

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public bool IsDeleted { get; set; } = false;
    
    public ICollection<Sale> Sales { get; set; }
}