using MediatR;
using VLauncher.Application.DTOs;
using VLauncher.Domain.Interfaces;

namespace VLauncher.Application.Users.Queries;

public record GetAvailableSecurityGroupsQuery : IRequest<IEnumerable<AdSecurityGroupDto>>;

public class GetAvailableSecurityGroupsQueryHandler : IRequestHandler<GetAvailableSecurityGroupsQuery, IEnumerable<AdSecurityGroupDto>>
{
    private readonly IActiveDirectoryService _adService;

    public GetAvailableSecurityGroupsQueryHandler(IActiveDirectoryService adService)
    {
        _adService = adService;
    }

    public async Task<IEnumerable<AdSecurityGroupDto>> Handle(GetAvailableSecurityGroupsQuery request, CancellationToken cancellationToken)
    {
        var groups = await _adService.GetSecurityGroupsFromOuAsync();

        return groups.Select(g => new AdSecurityGroupDto
        {
            DistinguishedName = g.DistinguishedName,
            Name = g.Name,
            Description = g.Description
        });
    }
}
