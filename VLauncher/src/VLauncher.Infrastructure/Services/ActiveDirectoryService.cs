using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using VLauncher.Domain.Entities;
using VLauncher.Domain.Interfaces;

namespace VLauncher.Infrastructure.Services;

public class ActiveDirectoryService : IActiveDirectoryService
{
    private readonly ActiveDirectorySettings _settings;

    public ActiveDirectoryService(IOptions<ActiveDirectorySettings> settings)
    {
        _settings = settings.Value;
    }

    private PrincipalContext GetPrincipalContext(string? container = null)
    {
        return new PrincipalContext(
            ContextType.Domain,
            _settings.Server,
            container ?? _settings.UsersOu,
            _settings.AdminUsername,
            _settings.AdminPassword);
    }

    private PrincipalContext GetDomainContext()
    {
        return new PrincipalContext(
            ContextType.Domain,
            _settings.Server,
            null,
            _settings.AdminUsername,
            _settings.AdminPassword);
    }

    private DirectoryEntry GetDirectoryEntry(string? path = null)
    {
        var ldapPath = string.IsNullOrEmpty(path)
            ? $"LDAP://{_settings.Server}"
            : $"LDAP://{_settings.Server}/{path}";

        return new DirectoryEntry(ldapPath, _settings.AdminUsername, _settings.AdminPassword);
    }

    //Validate Credentials (Admin Login)

    public Task<bool> ValidateCredentialsAsync(string username, string password)
    {
        try
        {
            using var context = GetDomainContext();
            return Task.FromResult(context.ValidateCredentials(username, password));
        }
        catch
        {
            return Task.FromResult(false);
        }
    }


    // Check if User is Admin
    public Task<bool> IsUserInGroupAsync(string username, string groupName)
    {
        try
        {
            // Use domain-wide context to find users anywhere in AD
            using var context = GetDomainContext();
            using var user = UserPrincipal.FindByIdentity(context, username);

            if (user == null)
                return Task.FromResult(false);

            using var group = GroupPrincipal.FindByIdentity(context, groupName);

            if (group == null)
                return Task.FromResult(false);

            return Task.FromResult(user.IsMemberOf(group));
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    //Get User with LDAP Search
    public Task<AdUser?> GetUserAsync(string userPrincipalName)
    {
        try
        {
            // Use DirectorySearcher to search within the VLauncher OU
            using var entry = GetDirectoryEntry(_settings.UsersOu);
            using var searcher = new DirectorySearcher(entry)
            {
                Filter = $"(&(objectClass=user)(userPrincipalName={userPrincipalName}))",
                SearchScope = SearchScope.Subtree
            };

            searcher.PropertiesToLoad.Add("userPrincipalName");
            searcher.PropertiesToLoad.Add("sAMAccountName");
            searcher.PropertiesToLoad.Add("displayName");
            searcher.PropertiesToLoad.Add("mail");
            searcher.PropertiesToLoad.Add("distinguishedName");
            searcher.PropertiesToLoad.Add("userAccountControl");
            searcher.PropertiesToLoad.Add("memberOf");

            var result = searcher.FindOne();

            if (result == null)
                return Task.FromResult<AdUser?>(null);

            var props = result.Properties;
            var userAccountControl = props["userAccountControl"].Count > 0
                ? (int)props["userAccountControl"][0]
                : 0;
            var isEnabled = (userAccountControl & 0x2) == 0; // 0x2 = ACCOUNTDISABLE

            var memberOf = new List<string>();
            if (props["memberOf"].Count > 0)
            {
                foreach (var group in props["memberOf"])
                {
                    memberOf.Add(group?.ToString() ?? string.Empty);
                }
            }

            var adUser = new AdUser
            {
                UserPrincipalName = props["userPrincipalName"].Count > 0 ? props["userPrincipalName"][0]?.ToString() ?? string.Empty : string.Empty,
                SamAccountName = props["sAMAccountName"].Count > 0 ? props["sAMAccountName"][0]?.ToString() ?? string.Empty : string.Empty,
                DisplayName = props["displayName"].Count > 0 ? props["displayName"][0]?.ToString() ?? string.Empty : string.Empty,
                Email = props["mail"].Count > 0 ? props["mail"][0]?.ToString() : null,
                DistinguishedName = props["distinguishedName"].Count > 0 ? props["distinguishedName"][0]?.ToString() ?? string.Empty : string.Empty,
                Enabled = isEnabled,
                MemberOf = memberOf
            };

            return Task.FromResult<AdUser?>(adUser);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting AD user: {ex.Message}");
            return Task.FromResult<AdUser?>(null);
        }
    }

    public Task<IEnumerable<AdSecurityGroup>> GetSecurityGroupsFromOuAsync()
    {
        var groups = new List<AdSecurityGroup>();

        try
        {
            using var entry = GetDirectoryEntry(_settings.SecurityGroupsOu);
            using var searcher = new DirectorySearcher(entry)
            {
                Filter = "(objectClass=group)",
                SearchScope = SearchScope.Subtree
            };

            searcher.PropertiesToLoad.Add("cn");
            searcher.PropertiesToLoad.Add("distinguishedName");
            searcher.PropertiesToLoad.Add("description");

            var results = searcher.FindAll();

            foreach (SearchResult result in results)
            {
                groups.Add(new AdSecurityGroup
                {
                    Name = result.Properties["cn"][0]?.ToString() ?? string.Empty,
                    DistinguishedName = result.Properties["distinguishedName"][0]?.ToString() ?? string.Empty,
                    Description = result.Properties["description"].Count > 0
                        ? result.Properties["description"][0]?.ToString()
                        : null
                });
            }
        }
        catch
        {
            // Log error in production
        }

        return Task.FromResult<IEnumerable<AdSecurityGroup>>(groups);
    }

    public Task<IEnumerable<AdSecurityGroup>> GetUserGroupsAsync(string userPrincipalName)
    {
        var groups = new List<AdSecurityGroup>();

        try
        {
            // Use DirectorySearcher to get user's memberOf attribute
            using var entry = GetDirectoryEntry(_settings.UsersOu);
            using var searcher = new DirectorySearcher(entry)
            {
                Filter = $"(&(objectClass=user)(userPrincipalName={userPrincipalName}))",
                SearchScope = SearchScope.Subtree
            };

            searcher.PropertiesToLoad.Add("memberOf");

            var result = searcher.FindOne();

            if (result == null)
            {
                Console.WriteLine($"GetUserGroupsAsync: User {userPrincipalName} not found in VLauncher OU");
                return Task.FromResult<IEnumerable<AdSecurityGroup>>(groups);
            }

            var memberOf = result.Properties["memberOf"];
            Console.WriteLine($"GetUserGroupsAsync: User {userPrincipalName} has {memberOf.Count} groups");

            foreach (var groupDn in memberOf)
            {
                var groupDnStr = groupDn?.ToString() ?? string.Empty;
                Console.WriteLine($"GetUserGroupsAsync: Group DN = {groupDnStr}");

                // Extract CN from DN (e.g., "CN=Vlauncher-Test,OU=VLauncher,DC=kaushalya,DC=local" -> "Vlauncher-Test")
                var cnMatch = System.Text.RegularExpressions.Regex.Match(groupDnStr, @"^CN=([^,]+)");
                var groupName = cnMatch.Success ? cnMatch.Groups[1].Value : groupDnStr;

                groups.Add(new AdSecurityGroup
                {
                    Name = groupName,
                    DistinguishedName = groupDnStr,
                    Description = null
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"GetUserGroupsAsync error: {ex.Message}");
        }

        return Task.FromResult<IEnumerable<AdSecurityGroup>>(groups);
    }

    // Add User to Group
    public Task AddUserToGroupAsync(string userPrincipalName, string groupDistinguishedName)
    {
        using var context = GetPrincipalContext();
        using var user = UserPrincipal.FindByIdentity(context, IdentityType.UserPrincipalName, userPrincipalName);

        if (user == null)
            throw new InvalidOperationException($"User {userPrincipalName} not found");

        using var group = GroupPrincipal.FindByIdentity(context, IdentityType.DistinguishedName, groupDistinguishedName);

        if (group == null)
            throw new InvalidOperationException($"Group {groupDistinguishedName} not found");

        if (!user.IsMemberOf(group))
        {
            group.Members.Add(user);
            group.Save();
        }

        return Task.CompletedTask;
    }

    public Task RemoveUserFromGroupAsync(string userPrincipalName, string groupDistinguishedName)
    {
        using var context = GetPrincipalContext();
        using var user = UserPrincipal.FindByIdentity(context, IdentityType.UserPrincipalName, userPrincipalName);

        if (user == null)
            throw new InvalidOperationException($"User {userPrincipalName} not found");

        using var group = GroupPrincipal.FindByIdentity(context, IdentityType.DistinguishedName, groupDistinguishedName);

        if (group == null)
            throw new InvalidOperationException($"Group {groupDistinguishedName} not found");

        if (user.IsMemberOf(group))
        {
            group.Members.Remove(user);
            group.Save();
        }

        return Task.CompletedTask;
    }

    public Task<string> ResetPasswordAsync(string userPrincipalName)
    {
        using var context = GetPrincipalContext();
        using var user = UserPrincipal.FindByIdentity(context, IdentityType.UserPrincipalName, userPrincipalName);

        if (user == null)
            throw new InvalidOperationException($"User {userPrincipalName} not found");

        var newPassword = GeneratePassword();
        user.SetPassword(newPassword);
        user.Save();

        return Task.FromResult(newPassword);
    }


    // Create New User
    public Task<AdUser> CreateUserAsync(string userPrincipalName, string displayName, string email)
    {
        using var context = GetPrincipalContext();

        // Extract username from UPN
        var samAccountName = userPrincipalName.Split('@')[0];

        using var user = new UserPrincipal(context)
        {
            UserPrincipalName = userPrincipalName,
            SamAccountName = samAccountName,
            DisplayName = displayName,
            EmailAddress = email,
            Enabled = true
        };

        var password = GeneratePassword();
        user.SetPassword(password);
        user.Save();

        return Task.FromResult(new AdUser
        {
            UserPrincipalName = user.UserPrincipalName ?? string.Empty,
            SamAccountName = user.SamAccountName ?? string.Empty,
            DisplayName = user.DisplayName ?? string.Empty,
            Email = user.EmailAddress,
            DistinguishedName = user.DistinguishedName ?? string.Empty,
            Enabled = user.Enabled ?? false,
            MemberOf = new List<string>()
        });
    }

    private static string GeneratePassword()
    {
        const string upperCase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string lowerCase = "abcdefghijklmnopqrstuvwxyz";
        const string digits = "0123456789";
        const string special = "!@#$%^&*";

        var password = new char[12];

        // Ensure at least one of each type
        password[0] = upperCase[RandomNumberGenerator.GetInt32(upperCase.Length)];
        password[1] = lowerCase[RandomNumberGenerator.GetInt32(lowerCase.Length)];
        password[2] = digits[RandomNumberGenerator.GetInt32(digits.Length)];
        password[3] = special[RandomNumberGenerator.GetInt32(special.Length)];

        var allChars = upperCase + lowerCase + digits + special;
        for (int i = 4; i < password.Length; i++)
        {
            password[i] = allChars[RandomNumberGenerator.GetInt32(allChars.Length)];
        }

        // Shuffle
        return new string(password.OrderBy(_ => RandomNumberGenerator.GetInt32(int.MaxValue)).ToArray());
    }
}
