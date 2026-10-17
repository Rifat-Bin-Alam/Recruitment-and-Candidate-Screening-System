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

        const string candidateEmail = "rifatcandidate@gmail.com";
        const string candidatePassword = "Cd@1234";

        var candidateUser = await userManager.FindByEmailAsync(candidateEmail);

        if (candidateUser == null)
        {
            candidateUser = new ApplicationUser
            {
                UserName = candidateEmail,
                Email = candidateEmail,
                EmailConfirmed = true
            };

            var candidateResult = await userManager.CreateAsync(candidateUser, candidatePassword);

            if (candidateResult.Succeeded)
            {
                await userManager.AddToRoleAsync(candidateUser, RoleNames.Candidate);
            }
        }
        else
        {
            if (!await userManager.IsInRoleAsync(candidateUser, RoleNames.Candidate))
            {
                await userManager.AddToRoleAsync(candidateUser, RoleNames.Candidate);
            }

            if (!await userManager.CheckPasswordAsync(candidateUser, candidatePassword))
            {
                var token = await userManager.GeneratePasswordResetTokenAsync(candidateUser);
                await userManager.ResetPasswordAsync(candidateUser, token, candidatePassword);
            }
        }
    }
}