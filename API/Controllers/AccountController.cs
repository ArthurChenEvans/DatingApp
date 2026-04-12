using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AccountController(UserManager<AppUser> userManager, ITokenService tokenService) : BaseApiController
{
    [HttpPost("register")]
    public async Task<ActionResult<UserDTO>> Register(RegisterDTO registerDTO)
    {
        var user = new AppUser
        {
            DisplayName = registerDTO.DisplayName,
            Email = registerDTO.Email,
            UserName = registerDTO.Email,
            Member = new Member
            {
                DisplayName = registerDTO.DisplayName,
                Gender = registerDTO.Gender,
                City = registerDTO.City,
                Country = registerDTO.Country,
                DateOfBirth = registerDTO.DateOfBirth
            }
        };
        
        var result = await userManager.CreateAsync(user, registerDTO.Password);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("identity", error.Description);
            }

            return ValidationProblem();
        }

        await userManager.AddToRoleAsync(user, "Member");

        await SetRefreshTokenCookie(user);

        return await user.ToDTO(tokenService);
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDTO>> Login(LoginDTO loginDto)
    {
        var user = await userManager.FindByEmailAsync(loginDto.Email);

        if (user == null) return Unauthorized("Invalid email address");
        
        var result = await userManager.CheckPasswordAsync(user, loginDto.Password);
        
        if (!result) return Unauthorized("Invalid password");

        await SetRefreshTokenCookie(user);

        return await user.ToDTO(tokenService);
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<UserDTO>> RefreshToken()
    {
        var refreshToken = Request.Cookies["refreshToken"];
        if (refreshToken == null) return NoContent();

        var user = await userManager.Users
            .FirstOrDefaultAsync(x => x.RefreshToken == refreshToken
                                      && x.RefreshTokenExpires > DateTime.UtcNow);

        if (user == null) return Unauthorized();

        await SetRefreshTokenCookie(user);

        return await user.ToDTO(tokenService);
    }

    private async Task SetRefreshTokenCookie(AppUser user)
    {
        var refreshToken = tokenService.GenerateRefreshToken();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpires = DateTime.UtcNow.AddDays(7);
        await userManager.UpdateAsync(user);

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(7)
        };
        
        Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
    }
    
    [Authorize]
    [HttpPost("logout")]
    public async Task<ActionResult> Logout()
    {
        await userManager.Users
            .Where(x => x.Id == User.GetMemberId())
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.RefreshToken, _ => null)
                .SetProperty(x => x.RefreshTokenExpires, _ => null)
            );

        Response.Cookies.Delete("refreshToken");

        return Ok();
    }
}