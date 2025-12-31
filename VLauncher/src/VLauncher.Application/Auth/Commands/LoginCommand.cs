using MediatR;
using VLauncher.Application.Common;
using VLauncher.Domain.Interfaces;

namespace VLauncher.Application.Auth.Commands;

public record LoginCommand(string Username, string Password) : IRequest<Result<LoginResult>>;

public class LoginResult
{
    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public bool IsAdmin { get; set; }
}

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResult>>
{
    private readonly IActiveDirectoryService _adService;
    private readonly string _adminGroupName;

    public LoginCommandHandler(
        IActiveDirectoryService adService,
        Microsoft.Extensions.Configuration.IConfiguration configuration)
    {
        _adService = adService;
        _adminGroupName = configuration["ActiveDirectory:AdminGroupName"] ?? "VLauncher-Admins";
    }

    public async Task<Result<LoginResult>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // Validate credentials against AD
        var isValid = await _adService.ValidateCredentialsAsync(request.Username, request.Password);

        if (!isValid)
            return Result<LoginResult>.Failure("Invalid username or password");

        // Check if user is in admin group
        var isAdmin = await _adService.IsUserInGroupAsync(request.Username, _adminGroupName);

        if (!isAdmin)
            return Result<LoginResult>.Failure("Access denied. You must be a member of the admin group.");

        // Get user details
        var adUser = await _adService.GetUserAsync($"{request.Username}@{GetDomainFromUsername(request.Username)}");

        return Result<LoginResult>.Success(new LoginResult
        {
            Username = request.Username,
            DisplayName = adUser?.DisplayName ?? request.Username,
            IsAdmin = isAdmin
        });
    }

    private string GetDomainFromUsername(string username)
    {
        // If username contains @, extract domain
        if (username.Contains('@'))
            return username.Split('@')[1];

        // Otherwise use configuration
        return "kaushalya.local";
    }
}
