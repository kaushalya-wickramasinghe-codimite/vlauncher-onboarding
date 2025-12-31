using MediatR;
using VLauncher.Application.Common;
using VLauncher.Domain.Enums;
using VLauncher.Domain.Interfaces;

namespace VLauncher.Application.Users.Commands;

public record RegisterUserCommand(
    int UserId,
    string AdUserPrincipalName,
    List<string> SecurityGroupDns) : IRequest<Result>;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IActiveDirectoryService _adService;

    public RegisterUserCommandHandler(IUnitOfWork unitOfWork, IActiveDirectoryService adService)
    {
        _unitOfWork = unitOfWork;
        _adService = adService;
    }

    public async Task<Result> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId);

        if (user == null)
            return Result.Failure("User not found");

        if (user.Status != UserStatus.Pending)
            return Result.Failure("User is already registered");

        // Verify AD user exists
        var adUser = await _adService.GetUserAsync(request.AdUserPrincipalName);
        if (adUser == null)
            return Result.Failure($"AD user {request.AdUserPrincipalName} not found");

        // Add user to selected security groups
        foreach (var groupDn in request.SecurityGroupDns)
        {
            try
            {
                await _adService.AddUserToGroupAsync(request.AdUserPrincipalName, groupDn);
            }
            catch (Exception ex)
            {
                return Result.Failure($"Failed to add user to group: {ex.Message}");
            }
        }

        // Update user in database
        user.AdUserPrincipalName = request.AdUserPrincipalName;
        user.Status = UserStatus.Registered;
        user.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success();
    }
}
