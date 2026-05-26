using PhilCharronWebFolio.Application.Auth.DTOs;
using PhilCharronWebFolio.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace PhilCharronWebFolio.Application.Common.Interfaces;

public interface ICurrentUserService { Guid? UserId { get; } }
public interface ITokenService { Task<string> GenerateTokenAsync(Guid userId, string email, string userName, IList<string> roles); }
public interface IIdentityService
{
    Task<AuthResponseDto> LoginAsync(string loginOrEmail, string password, CancellationToken ct);
    Task<AuthResponseDto> RegisterAsync(string firstName, string lastName, string userName, string email, string password, CancellationToken ct);
    Task<ProfileDto> GetProfileAsync(Guid userId, CancellationToken ct);
}

//public interface IUserProfileRepository
//{
//    Task AddAsync(UserProfile userProfile, CancellationToken ct);
//    Task<UserProfile?> GetByIdentityIdAsync(Guid identityId, CancellationToken ct);
//    Task SaveChangesAsync(CancellationToken ct);
//}

//public interface IBugReportRepository
//{
//    Task AddAsync(BugReport bugReport, CancellationToken ct);
//    Task SaveChangesAsync(CancellationToken ct);
//}
