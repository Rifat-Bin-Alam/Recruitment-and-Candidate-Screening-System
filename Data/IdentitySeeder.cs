using Microsoft.AspNetCore.Identity;
using RecruitmentAndCandidateScreeningSystem.Models;

namespace RecruitmentAndCandidateScreeningSystem.Data;

public static class IdentitySeeder
{
    public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        string[] roles =
        {
            RoleNames.Candidate,
            RoleNames.HRManager
        };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        const string hrEmail = "rifat@gmail.com";
        const string hrPassword = "Hr@123456";

        var hrUser = await userManager.FindByEmailAsync(hrEmail);

        if (hrUser == null)
        {
            hrUser = new ApplicationUser
            {
                UserName = hrEmail,
                Email = hrEmail,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(hrUser, hrPassword);

            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join("; ", result.Errors.Select(e => e.Description)));
            }
        }

        if (!await userManager.IsInRoleAsync(hrUser, RoleNames.HRManager))
        {
            await userManager.AddToRoleAsync(hrUser, RoleNames.HRManager);
        }
    }
}