using MediatR;
using VLauncher.Application.Common;
using VLauncher.Domain.Enums;
using VLauncher.Domain.Interfaces;

namespace VLauncher.Application.Users.Commands;

public record ResetUserPasswordCommand(int UserId) : IRequest<Result<string>>;

public class ResetUserPasswordCommandHandler : IRequestHandler<ResetUserPasswordCommand, Result<string>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IActiveDirectoryService _adService;

    public ResetUserPasswordCommandHandler(IUnitOfWork unitOfWork, IActiveDirectoryService adService)
    {
        _unitOfWork = unitOfWork;
        _adService = adService;
    }

    public async Task<Result<string>> Handle(ResetUserPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId);

        if (user == null)
            return Result<string>.Failure("User not found");

        if (user.Status != UserStatus.Registered || string.IsNullOrEmpty(user.AdUserPrincipalName))
            return Result<string>.Failure("User is not registered or has no AD account");

        try
        {
            var newPassword = await _adService.ResetPasswordAsync(user.AdUserPrincipalName);
            return Result<string>.Success(newPassword);
        }
        catch (Exception ex)
        {
            return Result<string>.Failure($"Failed to reset password: {ex.Message}");
        }
    }
}
