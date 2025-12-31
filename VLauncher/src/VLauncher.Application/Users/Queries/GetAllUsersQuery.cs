using MediatR;
using VLauncher.Application.DTOs;
using VLauncher.Domain.Interfaces;

namespace VLauncher.Application.Users.Queries;

public record GetAllUsersQuery : IRequest<IEnumerable<UserDto>>;

public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, IEnumerable<UserDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllUsersQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<UserDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _unitOfWork.Users.GetAllAsync();

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
