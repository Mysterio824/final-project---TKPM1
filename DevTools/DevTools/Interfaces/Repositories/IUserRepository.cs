// DevTools/Interfaces/Repositories/IUserRepository.cs
using DevTools.Entities;

namespace DevTools.Interfaces.Repositories;

public interface IUserRepository
{
    Task<User> GetByEmailAsync(string email);
    Task<User> GetByIdAsync(int id);
    Task AddAsync(User user);
    Task UpdateAsync(User user);
}