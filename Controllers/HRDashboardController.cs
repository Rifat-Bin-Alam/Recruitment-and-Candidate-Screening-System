using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecruitmentAndCandidateScreeningSystem.Data;
using RecruitmentAndCandidateScreeningSystem.Models;
using RecruitmentAndCandidateScreeningSystem.Models.Enums;
using RecruitmentAndCandidateScreeningSystem.ViewModels;

namespace RecruitmentAndCandidateScreeningSystem.Controllers;

[Authorize(Roles = RoleNames.HRManager)]
public class HRDashboardController : Controller
{
    private readonly ApplicationDbContext _context;

    public HRDashboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var totalApplications =
            await _context.JobApplications.CountAsync();

        var aiEvaluated =
            await _context.AIMatchingResults.CountAsync();

        var shortlisted =
            await _context.JobApplications.CountAsync(
                a => a.Status == JobApplicationStatus.Shortlisted);

        var manualReview =
            await _context.JobApplications.CountAsync(
                a => a.Status == JobApplicationStatus.ManualReview);

        var rejected =
            await _context.JobApplications.CountAsync(
                a => a.Status == JobApplicationStatus.Rejected);

        var paymentPending =
            await _context.Payments.CountAsync(
                p => p.Status == PaymentStatus.Pending);

        var publishedJobs =
            await _context.JobCirculars.CountAsync(
                j => j.Status == JobCircularStatus.Published);

        var recentApplications =
            await _context.JobApplications
                .AsNoTracking()
                .Include(a => a.CandidateProfile)
                    .ThenInclude(c => c.ApplicationUser)
                .Include(a => a.JobCircular)
                .Include(a => a.AIMatchingResult)
                .OrderByDescending(a => a.AppliedAt)
                .Take(5)
                .Select(a => new RecentApplicationViewModel
                {
                    Id = a.Id,
                    ApplicationReferenceId =
                        a.ApplicationReferenceId,

                    CandidateName =
                        a.CandidateProfile.FullName,

                    JobTitle =
                        a.JobCircular.JobTitle,

                    AppliedAt =
                        a.AppliedAt,

                    ApplicationStatus =
                        a.Status.ToString(),

                    AIMatchingScore =
                        a.AIMatchingResult != null
                            ? a.AIMatchingResult.MatchingScore
                            : null,

                    AIDecision =
                        a.AIMatchingResult != null
                            ? a.AIMatchingResult.Decision
                            : null
                })
                .ToListAsync();

        var model = new HRDashboardViewModel
        {
            TotalApplications = totalApplications,
            AIEvaluated = aiEvaluated,
            Shortlisted = shortlisted,
            ManualReview = manualReview,
            Rejected = rejected,
            PaymentPending = paymentPending,
            PublishedJobs = publishedJobs,
            RecentApplications = recentApplications
        };

        return View(model);
    }
}