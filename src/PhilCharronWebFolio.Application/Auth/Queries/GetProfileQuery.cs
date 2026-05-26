using PhilCharronWebFolio.Application.Auth.DTOs;
using PhilCharronWebFolio.Application.Common.Interfaces;
using PhilCharronWebFolio.Application.Common.Messaging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PhilCharronWebFolio.Application.Auth.Queries;

public sealed record GetProfileQuery(Guid UserId) : IQuery<ProfileDto>;

public sealed class GetProfileQueryHandler(IIdentityService identityService) : IQueryHandler<GetProfileQuery, ProfileDto>
{
    public async Task<ProfileDto> HandleAsync(GetProfileQuery query, CancellationToken ct) =>
        await identityService.GetProfileAsync(query.UserId, ct);
}
