using DevTools.DataAccess.Persistence;
using DevTools.Domain.Entities;

namespace DevTools.DataAccess.Repositories.impl;

public class UserRepository(DatabaseContext context) : BaseRepository<User>(context), IUserRepository
{
    public async Task<User?> GetByEmailAsync(string email)
        => await GetFirstAsync(u => u.Email == email);

    public async Task<User?> GetByIdAsync(int id)
        => await GetFirstAsync(u => u.Id == id);
}