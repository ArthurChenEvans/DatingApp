using API.DTOs;
using API.Entities;
using API.Interfaces;

namespace API.Extensions;

public static class AppUserExtensions
{
    public static UserDTO ToDTO(this AppUser user, ITokenService tokenService)
    {
        return new UserDTO()
        {
            Id = user.Id,
            DisplayName = user.DisplayName,
            UserName = user.UserName,
            Email = user.Email,
            Token = tokenService.CreateToken(user)
        };
    }
}