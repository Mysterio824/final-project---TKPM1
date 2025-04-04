using Microsoft.EntityFrameworkCore;
using DevTools.Domain.Entities;
using DevTools.Infrastructure.Persistence;

namespace DevTools.Infrastructure.Repositories.impl;

public class UserRepository(DatabaseContext context) : IUserRepository
{
    private readonly DatabaseContext _context = context;

    public async Task<User?> GetByEmailAsync(string email)
        => await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

    public async Task<User?> GetByIdAsync(int id)
        => await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

    public async Task AddAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }
}