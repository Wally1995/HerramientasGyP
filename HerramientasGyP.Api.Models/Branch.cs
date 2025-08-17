using System.ComponentModel.DataAnnotations;
using HerramientasGyP.Api.Models.Inventories;

namespace HerramientasGyP.Api.Models;

public class Branch
{
    public Guid Id { get; set; }
    [Required]
    public string Name { get; set; } // e.g. "Herramientas GyP - León"
    [Required]
    public string IdentificationNumber { get; set; } // Legal ID (RUC/NIT/etc.)
    
    public ICollection<Warehouse> Warehouses { get; set; }
    public ICollection<ApplicationUser> Users { get; set; }
}