using DevTools.Domain.Entities;

namespace DevTools.DataAccess.Repositories;

public interface IUserRepository : IBaseRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(int id);
}