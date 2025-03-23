using DevTools.Enums;

namespace DevTools.DTOs.UserDtos;

public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string Token { get; set; }
    public string RefreshToken { get; set; }
    public UserRole Role { get; set; }
    public bool IsPremium { get; set; }
}