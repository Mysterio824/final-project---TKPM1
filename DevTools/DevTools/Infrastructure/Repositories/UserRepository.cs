using Microsoft.EntityFrameworkCore;
using DevTools.Application.Interfaces.Repositories;
using DevTools.Domain.Entities;
using DevTools.Infrastructure.Data;

namespace DevTools.Infrastructure.Repositories;

public class UserRepository(ApplicationDbContext context) : IUserRepository
{
    private readonly ApplicationDbContext _context = context;

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