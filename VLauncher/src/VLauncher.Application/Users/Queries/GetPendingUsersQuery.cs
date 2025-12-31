using MediatR;
using VLauncher.Application.DTOs;
using VLauncher.Domain.Enums;
using VLauncher.Domain.Interfaces;

namespace VLauncher.Application.Users.Queries;

public record GetPendingUsersQuery : IRequest<IEnumerable<UserDto>>;

public class GetPendingUsersQueryHandler : IRequestHandler<GetPendingUsersQuery, IEnumerable<UserDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetPendingUsersQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<UserDto>> Handle(GetPendingUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _unitOfWork.Users.GetByStatusAsync(UserStatus.Pending);

        return users.Select(u => new UserDto
        {
            Id = u.Id,
            GoogleEmail = u.GoogleEmail,
            AdUserPrincipalName = u.AdUserPrincipalName,
            Status = u.Status,
            CreatedAt = u.CreatedAt
        });
    }
}
