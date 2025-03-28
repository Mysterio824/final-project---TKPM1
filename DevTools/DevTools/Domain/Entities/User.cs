using DevTools.Domain.Enums;

namespace DevTools.Domain.Entities;
public class User
{
    public int Id { get; set; }
    public required string Username { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public UserRole Role { get; set; } = UserRole.User;
    public bool IsPremium { get; set; } = false;
}