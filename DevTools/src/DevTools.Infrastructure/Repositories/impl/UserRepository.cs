using DevTools.Domain.Entities;
using DevTools.Infrastructure.Persistence;

namespace DevTools.Infrastructure.Repositories.impl;

public class UserRepository(DatabaseContext context) : BaseRepository<User>(context), IUserRepository
{
    public async Task<User?> GetByEmailAsync(string email)
        => await GetFirstAsync(u => u.Email == email);

    public async Task<User?> GetByIdAsync(int id)
        => await GetFirstAsync(u => u.Id == id);
}