using VLauncher.Domain.Enums;

namespace VLauncher.Domain.Entities;

public class User
{
    public int Id { get; set; }
    public string GoogleEmail { get; set; } = string.Empty;
    public string? AdUserPrincipalName { get; set; }
    public UserStatus Status { get; set; } = UserStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
