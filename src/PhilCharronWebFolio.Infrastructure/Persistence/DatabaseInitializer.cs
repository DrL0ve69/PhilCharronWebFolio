using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using PhilCharronWebFolio.Domain.Constants;
using PhilCharronWebFolio.Infrastructure.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace PhilCharronWebFolio.Infrastructure.Persistence;

public static class DatabaseInitializer
{
    public static async Task InitializeRolesAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();

        string[] roles = { Roles.Member, Roles.Admin, Roles.SuperAdmin };

        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
            }
        }

        var adminEmail = "admin@philcharron.web";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser is null)
        {
            adminUser = new AppUser
            {
                UserName = "superadmin",
                Email = adminEmail,
                FirstName = "Super",
                LastName = "Admin",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, "Password123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, Roles.SuperAdmin);
            }
        }
    }
}
