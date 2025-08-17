using System.ComponentModel.DataAnnotations;

namespace HerramientasGyP.Api.Models;

public class Person
{
    public int PersonId { get; set; } // Primary Key
    
    [MaxLength(50)]
    [Required]
    public string FirstName { get; set; } // Required
    
    [MaxLength(50)]
    [Required]
    public string LastName { get; set; } // Required
    
    [MaxLength(1)]
    [Required]
    public string Gender { get; set; } // Optional
    
    public DateTime? DateOfBirth { get; set; } // Optional, nullable for flexibility
    
    [MaxLength(14)]
    [Required]
    public string DocumentId { get; set; } // Nullable, required for employees (e.g., passport)
    
    [MaxLength(1)]
    [Required]
    public string Status { get; set; } = "A"; // Active or Erased
    
    [Required]
    public string? ApplicationUserId { get; set; } // Foreign Key to AspNetUsers for login-enabled users
    public ApplicationUser? ApplicationUser { get; set; } // Optional relationship with Identity user
}