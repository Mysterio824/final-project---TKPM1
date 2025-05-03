using DevTools.Domain.Common;
using DevTools.Domain.Enums;

namespace DevTools.Domain.Entities;
public class User : BaseEntity
{
    public required string Username { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public UserRole Role { get; set; } = UserRole.User;
}