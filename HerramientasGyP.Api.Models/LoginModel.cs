using System.ComponentModel.DataAnnotations;

namespace HerramientasGyP.Api.Models;

public class LoginModel
{
    [MaxLength(14)]
    [Required (ErrorMessage = "Requerido")]
    public string PersonDocumentId { get; set; }

    [Required(ErrorMessage = "Requerido")]
    public string ApplicationUserEmail { get; set; }
}