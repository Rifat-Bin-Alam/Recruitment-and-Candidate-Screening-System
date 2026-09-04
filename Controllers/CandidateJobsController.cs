using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecruitmentAndCandidateScreeningSystem.Data;
using RecruitmentAndCandidateScreeningSystem.Models;
using RecruitmentAndCandidateScreeningSystem.Models.Enums;
using RecruitmentAndCandidateScreeningSystem.Services;
using RecruitmentAndCandidateScreeningSystem.ViewModels;

namespace RecruitmentAndCandidateScreeningSystem.Controllers;

[Authorize(Roles = RoleNames.Candidate)]
public class CandidateJobsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly StripePaymentService _stripePaymentService;

    public CandidateJobsController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        StripePaymentService stripePaymentService)
    {
        _context = context;
        _userManager = userManager;
        _stripePaymentService = stripePaymentService;
    }

    public async Task<IActionResult> Index(
    string? search,
    string? sort)
    {
        var jobsQuery = _context.JobCirculars
            .Where(j =>
                j.Status == JobCircularStatus.Published &&
                j.ApplicationDeadline > DateTime.UtcNow);

        // Search
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();

            jobsQuery = jobsQuery.Where(j =>
                j.Title.Contains(search) ||
                (j.Department != null &&
                 j.Department.Contains(search)) ||
                (j.Location != null &&
                 j.Location.Contains(search)));
        }

        // Sorting
        jobsQuery = sort switch
        {
            "oldest" =>
                jobsQuery.OrderBy(j => j.PublishedAt),

            "deadline_soon" =>
                jobsQuery.OrderBy(j => j.ApplicationDeadline),

            "deadline_late" =>
                jobsQuery.OrderByDescending(j => j.ApplicationDeadline),

            _ =>
                jobsQuery.OrderByDescending(j => j.PublishedAt)
        };

        var jobs = await jobsQuery.ToListAsync();

        ViewBag.Search = search;
        ViewBag.Sort = sort ?? "newest";

        return View(
            "~/Views/Candidate/Jobs.cshtml",
            jobs);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
            return NotFound();

        var job = await _context.JobCirculars
            .Include(j => j.JobRequirement)
            .FirstOrDefaultAsync(j =>
                j.Id == id &&
                j.Status == JobCircularStatus.Published &&
                j.ApplicationDeadline > DateTime.UtcNow);

        if (job == null)
            return NotFound();

        var userId = _userManager.GetUserId(User);

        if (userId != null)
        {
            var profileId = await _context.CandidateProfiles
                .Where(p => p.ApplicationUserId == userId)
                .Select(p => (int?)p.Id)
                .FirstOrDefaultAsync();

            if (profileId.HasValue)
            {
                var existingApplication =
                    await _context.JobApplications
                        .Include(a => a.Payment)
                        .FirstOrDefaultAsync(a =>
                            a.CandidateProfileId == profileId.Value &&
                            a.JobCircularId == job.Id);

                ViewBag.ExistingApplication =
                    existingApplication;
            }
        }

        return View(
            "~/Views/Candidate/JobDetails.cshtml",
            job);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Apply(int id)
    {
        var userId = _userManager.GetUserId(User);

        if (userId == null)
            return Challenge();

        var profile = await _context.CandidateProfiles
            .Include(p => p.CVs)
            .FirstOrDefaultAsync(p =>
                p.ApplicationUserId == userId);

        if (profile == null)
        {
            TempData["Error"] =
                "Please complete your candidate profile before applying.";

            return RedirectToAction(
                nameof(Details),
                new { id });
        }

        var currentCV = profile.CVs
            .FirstOrDefault(c => c.IsCurrent);

        if (currentCV == null)
        {
            TempData["Error"] =
                "Please upload a CV before applying for a job.";

            return RedirectToAction(
                nameof(Details),
                new { id });
        }

        var job = await _context.JobCirculars
            .FirstOrDefaultAsync(j =>
                j.Id == id &&
                j.Status == JobCircularStatus.Published &&
                j.ApplicationDeadline > DateTime.UtcNow);

        if (job == null)
            return NotFound();

        var existingApplication =
            await _context.JobApplications
                .Include(a => a.Payment)
                .FirstOrDefaultAsync(a =>
                    a.CandidateProfileId == profile.Id &&
                    a.JobCircularId == job.Id);

        if (existingApplication != null)
        {
            if (existingApplication.Payment?.Status ==
                PaymentStatus.Pending)
            {
                return RedirectToAction(
                    nameof(PayNow),
                    new { id = existingApplication.Id });
            }

            TempData["Error"] =
                "You have already applied for this job.";

            return RedirectToAction(
                nameof(Details),
                new { id });
        }

        var application = new JobApplication
        {
            ApplicationReferenceId =
                $"APP-{Guid.NewGuid():N}",

            CandidateProfileId = profile.Id,
            JobCircularId = job.Id,
            CVId = currentCV.Id,

            Status = JobApplicationStatus.Submitted,

            AppliedAt = DateTime.UtcNow
        };

        _context.JobApplications.Add(application);

        // ==========================================
        // PAID JOB
        // ==========================================
        if (job.ApplicationFee.HasValue &&
            job.ApplicationFee.Value > 0)
        {
            var payment = new Payment
            {
                JobApplication = application,

                Amount = job.ApplicationFee.Value,

                PaymentMethod = PaymentMethod.Stripe,

                Status = PaymentStatus.Pending,

                CreatedAt = DateTime.UtcNow
            };

            _context.Payments.Add(payment);

            await _context.SaveChangesAsync();

            var user =
                await _userManager.GetUserAsync(User);

            var successUrl = Url.Action(
                nameof(PaymentSuccess),
                "CandidateJobs",
                new { id = application.Id },
                Request.Scheme)!;

            var cancelUrl = Url.Action(
                nameof(PaymentCancelled),
                "CandidateJobs",
                new { id = application.Id },
                Request.Scheme)!;

            var session =
                await _stripePaymentService
                    .CreateCheckoutSessionAsync(
                        application.ApplicationReferenceId,
                        job.Title,
                        job.ApplicationFee.Value,
                        user?.Email ?? string.Empty,
                        successUrl,
                        cancelUrl);

            return Redirect(session.Url);
        }

        // ==========================================
        // FREE JOB
        // ==========================================
        await _context.SaveChangesAsync();

        TempData["Success"] =
            $"Application submitted successfully. " +
            $"Reference ID: {application.ApplicationReferenceId}";

        return RedirectToAction(
            nameof(Details),
            new { id });
    }

    // ==============================================
    // PAY NOW
    // ==============================================
    [HttpGet]
    public async Task<IActionResult> PayNow(int id)
    {
        var userId = _userManager.GetUserId(User);

        if (userId == null)
            return Challenge();

        var application =
            await _context.JobApplications
                .Include(a => a.JobCircular)
                .Include(a => a.Payment)
                .Include(a => a.CandidateProfile)
                .FirstOrDefaultAsync(a =>
                    a.Id == id &&
                    a.CandidateProfile.ApplicationUserId ==
                    userId);

        if (application == null)
            return NotFound();

        if (application.Payment == null ||
            application.Payment.Status !=
            PaymentStatus.Pending)
        {
            return RedirectToAction(
                nameof(MyApplications));
        }

        var user =
            await _userManager.GetUserAsync(User);

        var successUrl = Url.Action(
            nameof(PaymentSuccess),
            "CandidateJobs",
            new { id = application.Id },
            Request.Scheme)!;

        var cancelUrl = Url.Action(
            nameof(PaymentCancelled),
            "CandidateJobs",
            new { id = application.Id },
            Request.Scheme)!;

        var session =
            await _stripePaymentService
                .CreateCheckoutSessionAsync(
                    application.ApplicationReferenceId,
                    application.JobCircular!.Title,
                    application.Payment.Amount,
                    user?.Email ?? string.Empty,
                    successUrl,
                    cancelUrl);

        return Redirect(session.Url);
    }

    // ==============================================
    // MY APPLICATIONS
    // ==============================================
    public async Task<IActionResult> MyApplications()
    {
        var userId = _userManager.GetUserId(User);

        if (userId == null)
            return Challenge();

        var applications =
            await _context.JobApplications
                .Include(a => a.JobCircular)
                .Include(a => a.Payment)
                .Include(a => a.CandidateProfile)
                .Where(a =>
                    a.CandidateProfile.ApplicationUserId ==
                    userId)
                .OrderByDescending(a => a.AppliedAt)
                .Select(a => new CandidateApplicationViewModel
                {
                    Id = a.Id,

                    JobTitle =
                        a.JobCircular!.Title,

                    // Do not show reference ID
                    // until payment is completed.
                    ApplicationReferenceId =
                        a.Payment != null &&
                        a.Payment.Status !=
                        PaymentStatus.Paid
                            ? string.Empty
                            : a.ApplicationReferenceId,

                    AppliedAt = a.AppliedAt,

                    ApplicationStatus =
                        a.Payment != null &&
                        a.Payment.Status !=
                        PaymentStatus.Paid
                            ? "Payment Pending"
                            : a.Status.ToString(),

                    HasPayment =
                        a.Payment != null,

                    PaymentStatus =
                        a.Payment == null
                            ? "Not Required"
                            : a.Payment.Status.ToString(),

                    PaymentAmount =
                        a.Payment == null
                            ? null
                            : a.Payment.Amount,

                    CanPay =
                        a.Payment != null &&
                        a.Payment.Status ==
                        PaymentStatus.Pending
                })
                .ToListAsync();

        return View(
            "~/Views/Candidate/MyApplications.cshtml",
            applications);
    }

    // ==============================================
    // STRIPE PAYMENT SUCCESS
    // ==============================================
    public async Task<IActionResult> PaymentSuccess(
        int id,
        string? session_id)
    {
        if (string.IsNullOrWhiteSpace(session_id))
        {
            TempData["Error"] =
                "Payment verification information is missing.";

            return RedirectToAction(
                nameof(MyApplications));
        }

        var userId = _userManager.GetUserId(User);

        if (userId == null)
            return Challenge();

        var application =
            await _context.JobApplications
                .Include(a => a.JobCircular)
                .Include(a => a.Payment)
                .Include(a => a.CandidateProfile)
                .FirstOrDefaultAsync(a =>
                    a.Id == id &&
                    a.CandidateProfile.ApplicationUserId ==
                    userId);

        if (application == null)
            return NotFound();

        if (application.Payment == null)
        {
            TempData["Error"] =
                "No payment record was found.";

            return RedirectToAction(
                nameof(MyApplications));
        }

        try
        {
            var sessionService =
                new Stripe.Checkout.SessionService();

            var session =
                await sessionService.GetAsync(session_id);

            var referenceId =
                session.Metadata.TryGetValue(
                    "application_reference_id",
                    out var metadataReference)
                        ? metadataReference
                        : session.ClientReferenceId;

            // Make sure this Stripe session belongs
            // to this application.
            if (referenceId !=
                application.ApplicationReferenceId)
            {
                TempData["Error"] =
                    "Payment verification failed.";

                return RedirectToAction(
                    nameof(MyApplications));
            }

            // Stripe must confirm the payment.
            if (session.PaymentStatus != "paid")
            {
                TempData["Error"] =
                    "Payment has not been confirmed.";

                return RedirectToAction(
                    nameof(MyApplications));
            }

            // Mark payment as paid.
            application.Payment.Status =
                PaymentStatus.Paid;

            application.Payment.PaidAt =
                DateTime.UtcNow;

            application.Payment.TransactionId =
                session.PaymentIntentId;

            await _context.SaveChangesAsync();

            return View(
                "~/Views/Candidate/PaymentSuccess.cshtml",
                application);
        }
        catch (Stripe.StripeException)
        {
            TempData["Error"] =
                "Unable to verify the payment with Stripe.";

            return RedirectToAction(
                nameof(MyApplications));
        }
    }

    // ==============================================
    // STRIPE PAYMENT CANCELLED
    // ==============================================
    public async Task<IActionResult> PaymentCancelled(
        int id)
    {
        var userId = _userManager.GetUserId(User);

        if (userId == null)
            return Challenge();

        var application =
            await _context.JobApplications
                .Include(a => a.JobCircular)
                .Include(a => a.Payment)
                .Include(a => a.CandidateProfile)
                .FirstOrDefaultAsync(a =>
                    a.Id == id &&
                    a.CandidateProfile.ApplicationUserId ==
                    userId);

        if (application == null)
            return NotFound();

        return View(
            "~/Views/Candidate/PaymentCancelled.cshtml",
            application);
    }
}