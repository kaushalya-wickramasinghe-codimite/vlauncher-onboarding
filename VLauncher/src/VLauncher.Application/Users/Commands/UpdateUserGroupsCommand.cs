using MediatR;
using VLauncher.Application.Common;
using VLauncher.Domain.Enums;
using VLauncher.Domain.Interfaces;

namespace VLauncher.Application.Users.Commands;

public record UpdateUserGroupsCommand(
    int UserId,
    List<string> GroupsToAdd,
    List<string> GroupsToRemove) : IRequest<Result>;

public class UpdateUserGroupsCommandHandler : IRequestHandler<UpdateUserGroupsCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IActiveDirectoryService _adService;

    public UpdateUserGroupsCommandHandler(IUnitOfWork unitOfWork, IActiveDirectoryService adService)
    {
        _unitOfWork = unitOfWork;
        _adService = adService;
    }

    public async Task<Result> Handle(UpdateUserGroupsCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId);

        if (user == null)
            return Result.Failure("User not found");

        if (user.Status != UserStatus.Registered || string.IsNullOrEmpty(user.AdUserPrincipalName))
            return Result.Failure("User is not registered or has no AD account");

        // Add user to new groups
        foreach (var groupDn in request.GroupsToAdd)
        {
            try
            {
                await _adService.AddUserToGroupAsync(user.AdUserPrincipalName, groupDn);
            }
            catch (Exception ex)
            {
                return Result.Failure($"Failed to add user to group: {ex.Message}");
            }
        }

        // Remove user from groups
        foreach (var groupDn in request.GroupsToRemove)
        {
            try
            {
                await _adService.RemoveUserFromGroupAsync(user.AdUserPrincipalName, groupDn);
            }
            catch (Exception ex)
            {
                return Result.Failure($"Failed to remove user from group: {ex.Message}");
            }
        }

        user.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success();
    }
}
