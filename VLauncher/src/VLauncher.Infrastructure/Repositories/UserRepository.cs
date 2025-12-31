using Microsoft.EntityFrameworkCore;
using VLauncher.Domain.Entities;
using VLauncher.Domain.Enums;
using VLauncher.Domain.Interfaces;
using VLauncher.Infrastructure.Data;

namespace VLauncher.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task<User?> GetByGoogleEmailAsync(string googleEmail)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.GoogleEmail == googleEmail);
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _context.Users.ToListAsync();
    }

    public async Task<IEnumerable<User>> GetByStatusAsync(UserStatus status)
    {
        return await _context.Users
            .Where(u => u.Status == status)
            .ToListAsync();
    }

    public async Task<User> AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
        return user;
    }

    public Task UpdateAsync(User user)
    {
        _context.Users.Update(user);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user != null)
        {
            _context.Users.Remove(user);
        }
    }
}
