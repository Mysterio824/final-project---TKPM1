using DevTools.Enums;

namespace DevTools.Entities;
public class User
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public string RefreshToken { get; set; }
    public UserRole Role { get; set; }
    public bool IsPremium { get; set; }
    public bool IsEmailVerified { get; set; }
}