using System;
using HerramientasGyP.Api.Models.Dtos.Users;

namespace HerramientasGyP.Api.Services.Mappers
{
    public static class LoginUserDtoMapper
    {
        public static LoginUserDto Map(dynamic source)
        {
            return new LoginUserDto
            {
                Email = source.Email,
                DocumentId = source.Person.DocumentId,
            };
        }
        public static List<LoginUserDto> MapList(IEnumerable<dynamic> sourceList)
        {
            return sourceList.Select(Map).ToList();
        }
    }
}
