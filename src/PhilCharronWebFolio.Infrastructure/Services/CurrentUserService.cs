using Microsoft.AspNetCore.Http;
using PhilCharronWebFolio.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace PhilCharronWebFolio.Infrastructure.Services;

public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    // On extrait l'ID de l'utilisateur depuis le Claim NameIdentifier (donc le token JWT)
    public Guid? UserId
    {
        get
        {
            var id = httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(id, out var guid) ? guid : null;
        }
    }
}
