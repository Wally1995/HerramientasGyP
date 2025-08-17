using System;

namespace HerramientasGyP.Api.Models.Dtos.Users;

public class ConfirmLoginDto
{
    public Guid SessionId { get; set; }
    public string Token { get; set; } = string.Empty;
}
