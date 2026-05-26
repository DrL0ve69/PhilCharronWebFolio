using Microsoft.AspNetCore.Identity;
using PhilCharronWebFolio.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace PhilCharronWebFolio.Infrastructure.Identity;

public sealed class AppUser : IdentityUser<Guid>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
}
