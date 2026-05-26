using Microsoft.AspNetCore.Identity;
using PhilCharronWebFolio.Application.Auth.DTOs;
using PhilCharronWebFolio.Application.Common.Interfaces;
using PhilCharronWebFolio.Domain.Constants;
using PhilCharronWebFolio.Domain.Entities;
using PhilCharronWebFolio.Domain.Exceptions;
using PhilCharronWebFolio.Infrastructure.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace PhilCharronWebFolio.Infrastructure.Services;

public sealed class IdentityService(UserManager<AppUser> userManager, ITokenService tokenService) : IIdentityService
{
    public async Task<AuthResponseDto> LoginAsync(string loginOrEmail, string password, CancellationToken ct)
    {
        var user = loginOrEmail.Contains('@') ? await userManager.FindByEmailAsync(loginOrEmail) : await userManager.FindByNameAsync(loginOrEmail);

        if (user is null || !await userManager.CheckPasswordAsync(user, password))
            throw new UnauthorizedException("Identifiants invalides.");

        var roles = await userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? Roles.Member;
        var token = await tokenService.GenerateTokenAsync(user.Id, user.Email!, user.UserName!, roles);

        return new AuthResponseDto(token, user.Id, user.Email!, role, DateTime.UtcNow.AddHours(8));
    }

    public async Task<AuthResponseDto> RegisterAsync(string firstName, string lastName, string userName, string email, string password, CancellationToken ct)
    {
        var user = new AppUser { UserName = userName, Email = email, FirstName = firstName, LastName = lastName };
        var result = await userManager.CreateAsync(user, password);

        if (!result.Succeeded)
            throw new ConflictException(string.Join(", ", result.Errors.Select(e => e.Description)));

        await userManager.AddToRoleAsync(user, Roles.Member);

        var roles = await userManager.GetRolesAsync(user);
        var token = await tokenService.GenerateTokenAsync(user.Id, user.Email!, user.UserName!, roles);

        return new AuthResponseDto(token, user.Id, user.Email!, Roles.Member, DateTime.UtcNow.AddHours(8));
    }

    public async Task<ProfileDto> GetProfileAsync(Guid userId, CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null) throw new NotFoundException("User", userId);

        return new ProfileDto(
            user.Id,
            user.UserName!,
            user.Email!,
            user.FirstName,
            user.LastName,
            user.FullName
        );
    }
}
