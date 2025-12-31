namespace VLauncher.Domain.Entities;

public class AdSecurityGroup
{
    public string DistinguishedName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
