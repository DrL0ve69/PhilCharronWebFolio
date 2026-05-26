using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PhilCharronWebFolio.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PhilCharronWebFolio.Infrastructure.Services;

public sealed class TokenService(IConfiguration config) : ITokenService
{
    public async Task<string> GenerateTokenAsync(Guid userId, string email, string userName, IList<string> roles)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Name, userName),
            new(ClaimTypes.Email, email)
        };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    //public Task<string> GenerateTokenAsync(Guid userId, string email, string userName, IList<string> roles)
    //{
    //    throw new NotImplementedException();
    //}
}
