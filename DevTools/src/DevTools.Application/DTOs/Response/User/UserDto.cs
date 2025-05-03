using DevTools.Domain.Enums;

namespace DevTools.Application.DTOs.Response.User;

public class UserDto
{
    public int Id { get; set; }
    public string? Username { get; set; }
    public required string Email { get; set; }
    public string? Token { get; set; }
    public string? RefreshToken { get; set; }
    public UserRole Role { get; set; }
}