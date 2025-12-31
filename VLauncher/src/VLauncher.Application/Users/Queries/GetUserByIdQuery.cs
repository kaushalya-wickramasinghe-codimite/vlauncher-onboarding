using MediatR;
using VLauncher.Application.DTOs;
using VLauncher.Domain.Enums;
using VLauncher.Domain.Interfaces;

namespace VLauncher.Application.Users.Queries;

public record GetUserByIdQuery(int Id) : IRequest<UserDetailDto?>;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDetailDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IActiveDirectoryService _adService;

    public GetUserByIdQueryHandler(IUnitOfWork unitOfWork, IActiveDirectoryService adService)
    {
        _unitOfWork = unitOfWork;
        _adService = adService;
    }

    public async Task<UserDetailDto?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(request.Id);

        if (user == null)
            return null;

        var dto = new UserDetailDto
        {
            Id = user.Id,
            GoogleEmail = user.GoogleEmail,
            AdUserPrincipalName = user.AdUserPrincipalName,
            Status = user.Status,
            CreatedAt = user.CreatedAt
        };

        // If user is registered, get their AD groups
        if (user.Status == UserStatus.Registered && !string.IsNullOrEmpty(user.AdUserPrincipalName))
        {
            var adGroups = await _adService.GetUserGroupsAsync(user.AdUserPrincipalName);
            dto.AdGroups = adGroups.Select(g => new AdSecurityGroupDto
            {
                DistinguishedName = g.DistinguishedName,
                Name = g.Name,
                Description = g.Description
            }).ToList();
        }

        return dto;
    }
}
