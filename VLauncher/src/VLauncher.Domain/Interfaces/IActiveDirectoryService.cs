using VLauncher.Domain.Entities;

namespace VLauncher.Domain.Interfaces;

public interface IActiveDirectoryService
{
    Task<bool> ValidateCredentialsAsync(string username, string password); // Login check
    Task<bool> IsUserInGroupAsync(string username, string groupName);  // Is user admin?
    Task<AdUser?> GetUserAsync(string userPrincipalName); // Get user details
    Task<IEnumerable<AdSecurityGroup>> GetSecurityGroupsFromOuAsync(); // List all groups
    Task<IEnumerable<AdSecurityGroup>> GetUserGroupsAsync(string userPrincipalName); // User's groups
    Task AddUserToGroupAsync(string userPrincipalName, string groupDistinguishedName); // Add to group
    Task RemoveUserFromGroupAsync(string userPrincipalName, string groupDistinguishedName); // Remove from group
    Task<string> ResetPasswordAsync(string userPrincipalName); // Reset password
    Task<AdUser> CreateUserAsync(string userPrincipalName, string displayName, string email); // Create user
}
