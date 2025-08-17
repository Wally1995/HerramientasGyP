using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;

namespace HerramientasGyP.Api.Models;

public class ApplicationUser : IdentityUser
{
    [JsonIgnore]
    public Person? Person { get; set; } // Optional relationship with Person
    [Required]
    public Guid BranchId { get; set; }
    public Branch Branch { get; set; }
    public bool IsBlocked { get; set; }
    public bool IsPermanentlyBanned { get; set; } = false;
}