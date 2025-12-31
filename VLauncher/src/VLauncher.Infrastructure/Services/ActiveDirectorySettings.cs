namespace VLauncher.Infrastructure.Services;

public class ActiveDirectorySettings
{
    public string Server { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public string AdminUsername { get; set; } = string.Empty;
    public string AdminPassword { get; set; } = string.Empty;
    public string AdminGroupName { get; set; } = string.Empty;
    public string SecurityGroupsOu { get; set; } = string.Empty;
    public string UsersOu { get; set; } = string.Empty;
}
