using MediatR;
using VLauncher.Application.Common;
using VLauncher.Application.DTOs;
using VLauncher.Domain.Entities;
using VLauncher.Domain.Enums;
using VLauncher.Domain.Interfaces;

namespace VLauncher.Application.Users.Commands;

public record CreatePendingUserCommand(string GoogleEmail) : IRequest<Result<UserDto>>;

public class CreatePendingUserCommandHandler : IRequestHandler<CreatePendingUserCommand, Result<UserDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreatePendingUserCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<UserDto>> Handle(CreatePendingUserCommand request, CancellationToken cancellationToken)
    {
        // Check if user already exists
        var existingUser = await _unitOfWork.Users.GetByGoogleEmailAsync(request.GoogleEmail);
        if (existingUser != null)
        {
            return Result<UserDto>.Failure("User with this email already exists");
        }

        var user = new User
        {
            GoogleEmail = request.GoogleEmail,
            Status = UserStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        var dto = new UserDto
        {
            Id = user.Id,
            GoogleEmail = user.GoogleEmail,
            AdUserPrincipalName = user.AdUserPrincipalName,
            Status = user.Status,
            CreatedAt = user.CreatedAt
        };

        return Result<UserDto>.Success(dto);
    }
}
