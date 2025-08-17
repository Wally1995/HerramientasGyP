namespace HerramientasGyP.Api.Models.Dtos.Users;

public class ApplicationUserDto
{
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Gender { get; set; }
    public DateTime Birth { get; set; }
}