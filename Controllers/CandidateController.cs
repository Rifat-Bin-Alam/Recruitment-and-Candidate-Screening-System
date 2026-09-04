using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecruitmentAndCandidateScreeningSystem.Data;
using RecruitmentAndCandidateScreeningSystem.Models;
using RecruitmentAndCandidateScreeningSystem.ViewModels;

namespace RecruitmentAndCandidateScreeningSystem.Controllers;

[Authorize(Roles = RoleNames.Candidate)]
public class CandidateController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public CandidateController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Dashboard()
    {
        var userId = _userManager.GetUserId(User);

        if (userId == null)
            return Challenge();

        var profile = await _context.CandidateProfiles
            .Include(p => p.CVs)
            .FirstOrDefaultAsync(p => p.ApplicationUserId == userId);

        var model = new CandidateDashboardViewModel
        {
            HasProfile = profile != null,

            ProfileName = profile?.FullName ?? "Candidate",

            HasPhoto = !string.IsNullOrWhiteSpace(
                profile?.PhotoFilePath),

            HasSignature = !string.IsNullOrWhiteSpace(
                profile?.SignatureFilePath),

            CVCount = profile?.CVs.Count ?? 0,

            HasCurrentCV = profile?.CVs.Any(c => c.IsCurrent) ?? false,

            ApplicationCount = profile == null
                ? 0
                : await _context.JobApplications.CountAsync(a =>
                    a.CandidateProfileId == profile.Id)
        };

        return View(model);
    }
}