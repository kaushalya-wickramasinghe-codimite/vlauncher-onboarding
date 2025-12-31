using VLauncher.Domain.Entities;
using VLauncher.Domain.Enums;

namespace VLauncher.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByGoogleEmailAsync(string googleEmail);
    Task<IEnumerable<User>> GetAllAsync();
    Task<IEnumerable<User>> GetByStatusAsync(UserStatus status);
    Task<User> AddAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(int id);
}
