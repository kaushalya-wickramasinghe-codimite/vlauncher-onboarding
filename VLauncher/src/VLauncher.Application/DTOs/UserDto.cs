using VLauncher.Domain.Enums;

namespace VLauncher.Application.DTOs;

public class UserDto
{
    public int Id { get; set; }
    public string GoogleEmail { get; set; } = string.Empty;
    public string? AdUserPrincipalName { get; set; }
    public UserStatus Status { get; set; }
    public string StatusDisplay => Status == UserStatus.Pending ? "Pending" : "Registered";
    public DateTime CreatedAt { get; set; }
}
