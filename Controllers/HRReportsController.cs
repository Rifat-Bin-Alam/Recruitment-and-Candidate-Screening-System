using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecruitmentAndCandidateScreeningSystem.Data;
using RecruitmentAndCandidateScreeningSystem.Models;
using RecruitmentAndCandidateScreeningSystem.Models.Enums;
using RecruitmentAndCandidateScreeningSystem.ViewModels;

namespace RecruitmentAndCandidateScreeningSystem.Controllers;

[Authorize(Roles = RoleNames.HRManager)]
public class HRReportsController : Controller
{
    private readonly ApplicationDbContext _context;

    public HRReportsController(
        ApplicationDbContext context)
    {
        _context = context;
    }

    // ============================================================
    // RECRUITMENT REPORT
    // ============================================================

    public async Task<IActionResult> Index()
    {
        // --------------------------------------------------------
        // APPLICATION STATISTICS
        // --------------------------------------------------------

        var totalApplications =
            await _context.JobApplications
                .CountAsync();

        var aiEvaluated =
            await _context.AIMatchingResults
                .CountAsync();

        var shortlisted =
            await _context.JobApplications
                .CountAsync(a =>
                    a.Status ==
                    JobApplicationStatus.Shortlisted);

        var manualReview =
            await _context.JobApplications
                .CountAsync(a =>
                    a.Status ==
                    JobApplicationStatus.ManualReview);

        var rejected =
            await _context.JobApplications
                .CountAsync(a =>
                    a.Status ==
                    JobApplicationStatus.Rejected);

        var interviewCandidates =
            await _context.JobApplications
                .CountAsync(a =>
                    a.Status ==
                    JobApplicationStatus.Interview);

        // --------------------------------------------------------
        // OFFER STATISTICS
        // --------------------------------------------------------

        var totalOffers =
            await _context.Offers
                .CountAsync();

        var pendingOffers =
            await _context.Offers
                .CountAsync(o =>
                    o.Status ==
                    OfferStatus.Pending);

        var acceptedOffers =
            await _context.Offers
                .CountAsync(o =>
                    o.Status ==
                    OfferStatus.Accepted);

        var rejectedOffers =
            await _context.Offers
                .CountAsync(o =>
                    o.Status ==
                    OfferStatus.Rejected);

        // --------------------------------------------------------
        // PAYMENT STATISTICS
        // --------------------------------------------------------

        var paidApplications =
            await _context.Payments
                .CountAsync(p =>
                    p.Status ==
                    PaymentStatus.Paid);

        var pendingPayments =
            await _context.Payments
                .CountAsync(p =>
                    p.Status ==
                    PaymentStatus.Pending);

        // --------------------------------------------------------
        // JOB STATISTICS
        // --------------------------------------------------------

        var publishedJobs =
            await _context.JobCirculars
                .CountAsync(j =>
                    j.Status ==
                    JobCircularStatus.Published);

        // --------------------------------------------------------
        // JOB-WISE STATISTICS
        // --------------------------------------------------------

        var jobStatistics =
            await _context.JobCirculars
                .AsNoTracking()
                .OrderByDescending(j => j.CreatedAt)
                .Select(j => new JobRecruitmentReportItem
                {
                    JobCircularId = j.Id,

                    JobTitle = j.JobTitle,

                    TotalApplications =
                        j.JobApplications.Count(),

                    AIEvaluated =
                        j.JobApplications
                            .Count(a =>
                                a.AIMatchingResult != null),

                    Shortlisted =
                        j.JobApplications
                            .Count(a =>
                                a.Status ==
                                JobApplicationStatus.Shortlisted),

                    ManualReview =
                        j.JobApplications
                            .Count(a =>
                                a.Status ==
                                JobApplicationStatus.ManualReview),

                    Rejected =
                        j.JobApplications
                            .Count(a =>
                                a.Status ==
                                JobApplicationStatus.Rejected),

                    InterviewCandidates =
                        j.JobApplications
                            .Count(a =>
                                a.Status ==
                                JobApplicationStatus.Interview),

                    Offers =
                        j.JobApplications
                            .Count(a =>
                                a.Offer != null),

                    AcceptedOffers =
                        j.JobApplications
                            .Count(a =>
                                a.Offer != null &&
                                a.Offer.Status ==
                                OfferStatus.Accepted)
                })
                .ToListAsync();

        // --------------------------------------------------------
        // BUILD REPORT MODEL
        // --------------------------------------------------------

        var model = new RecruitmentReportViewModel
        {
            TotalApplications = totalApplications,

            AIEvaluated = aiEvaluated,

            Shortlisted = shortlisted,

            ManualReview = manualReview,

            Rejected = rejected,

            InterviewCandidates =
                interviewCandidates,

            TotalOffers = totalOffers,

            PendingOffers = pendingOffers,

            AcceptedOffers =
                acceptedOffers,

            RejectedOffers =
                rejectedOffers,

            PaidApplications =
                paidApplications,

            PendingPayments =
                pendingPayments,

            PublishedJobs =
                publishedJobs,

            JobStatistics =
                jobStatistics
        };

        return View(model);
    }
}