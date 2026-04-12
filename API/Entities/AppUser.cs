using Microsoft.AspNetCore.Identity;

namespace API.Entities;

public class AppUser : IdentityUser
{
    public string? ImageUrl { get; set; }
    public required string DisplayName { get; set; }
    
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpires { get; set; }
    
    public Member Member { get; set; } = null!;
}