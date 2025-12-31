namespace VLauncher.Domain.Entities;

public class AdUser
{
    public string UserPrincipalName { get; set; } = string.Empty;
    public string SamAccountName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string DistinguishedName { get; set; } = string.Empty;
    public bool Enabled { get; set; }
    public List<string> MemberOf { get; set; } = new();
}
