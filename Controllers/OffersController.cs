using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecruitmentAndCandidateScreeningSystem.Data;
using RecruitmentAndCandidateScreeningSystem.Models;
using RecruitmentAndCandidateScreeningSystem.Models.Enums;

namespace RecruitmentAndCandidateScreeningSystem.Controllers;

[Authorize(Roles = RoleNames.HRManager)]
public class OffersController : Controller
{
    private readonly ApplicationDbContext _context;

    public OffersController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Shows shortlisted and manual review candidates
    // who are eligible for an offer
    public async Task<IActionResult> Index()
    {
        var applications = await _context.JobApplications
            .AsNoTracking()
            .Include(a => a.CandidateProfile)
            .Include(a => a.JobCircular)
            .Include(a => a.Offer)
            .Include(a => a.Offer!.JoiningSchedule)
            .Where(a =>
                a.Status == JobApplicationStatus.Shortlisted ||
                a.Status == JobApplicationStatus.ManualReview)
            .OrderByDescending(a => a.AppliedAt)
            .ToListAsync();

        return View(applications);
    }

    // Create Offer page
    [HttpGet]
    public async Task<IActionResult> Create(int applicationId)
    {
        var application = await _context.JobApplications
            .Include(a => a.CandidateProfile)
            .Include(a => a.JobCircular)
            .Include(a => a.Offer)
            .FirstOrDefaultAsync(a => a.Id == applicationId);

        if (application == null)
        {
            return NotFound();
        }

        // Only shortlisted or manual review candidates
        // can receive an offer
        if (application.Status != JobApplicationStatus.Shortlisted &&
            application.Status != JobApplicationStatus.ManualReview)
        {
            TempData["Error"] =
                "Only shortlisted or manual review candidates can receive an offer.";

            return RedirectToAction(nameof(Index));
        }

        // Prevent duplicate offers
        if (application.Offer != null)
        {
            TempData["Error"] =
                "An offer already exists for this application.";

            return RedirectToAction(nameof(Index));
        }

        ViewBag.Application = application;

        return View(new Offer());
    }

    // Save Offer and HR-provided Joining Schedule
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        int applicationId,
        Offer model,
        DateTime? proposedJoiningDate)
    {
        var application = await _context.JobApplications
            .Include(a => a.Offer)
            .FirstOrDefaultAsync(a => a.Id == applicationId);

        if (application == null)
        {
            return NotFound();
        }

        // Only shortlisted or manual review candidates
        // can receive an offer
        if (application.Status != JobApplicationStatus.Shortlisted &&
            application.Status != JobApplicationStatus.ManualReview)
        {
            TempData["Error"] =
                "Only shortlisted or manual review candidates can receive an offer.";

            return RedirectToAction(nameof(Index));
        }

        // Prevent duplicate offers
        if (application.Offer != null)
        {
            TempData["Error"] =
                "An offer already exists for this application.";

            return RedirectToAction(nameof(Index));
        }

        // Joining date is required
        if (!proposedJoiningDate.HasValue)
        {
            ModelState.AddModelError(
                "proposedJoiningDate",
                "Please provide a joining date.");
        }

        // Joining date should not be in the past
        if (proposedJoiningDate.HasValue &&
            proposedJoiningDate.Value < DateTime.Now)
        {
            ModelState.AddModelError(
                "proposedJoiningDate",
                "Joining date cannot be in the past.");
        }

        if (!ModelState.IsValid)
        {
            var fullApplication = await _context.JobApplications
                .Include(a => a.CandidateProfile)
                .Include(a => a.JobCircular)
                .FirstOrDefaultAsync(a => a.Id == applicationId);

            ViewBag.Application = fullApplication;

            return View(model);
        }

        // Create Offer
        model.JobApplicationId = applicationId;
        model.OfferDate = DateTime.UtcNow;
        model.Status = OfferStatus.Pending;
        model.CandidateDecision = CandidateDecision.Pending;
        model.DecisionDate = null;

        _context.Offers.Add(model);

        try
        {
            await _context.SaveChangesAsync();

            // Create joining schedule provided by HR
            var joiningSchedule = new JoiningSchedule
            {
                OfferId = model.Id,

                // HR-provided joining date
                SelectedJoiningDate = proposedJoiningDate.Value,

                // Will be confirmed when candidate accepts
                ConfirmedJoiningDate = null,
                IsConfirmed = false,
                ConfirmedAt = null
            };

            _context.JoiningSchedules.Add(joiningSchedule);

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Offer and joining schedule created successfully.";

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(
                string.Empty,
                $"Unable to create offer: {ex.Message}");

            var fullApplication = await _context.JobApplications
                .Include(a => a.CandidateProfile)
                .Include(a => a.JobCircular)
                .FirstOrDefaultAsync(a => a.Id == applicationId);

            ViewBag.Application = fullApplication;

            return View(model);
        }
    }

    // HR views a specific offer
    public async Task<IActionResult> Details(int id)
    {
        var offer = await _context.Offers
            .AsNoTracking()
            .Include(o => o.JobApplication)
                .ThenInclude(a => a.CandidateProfile)
            .Include(o => o.JobApplication)
                .ThenInclude(a => a.JobCircular)
            .Include(o => o.JoiningSchedule)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (offer == null)
        {
            return NotFound();
        }

        return View(offer);
    }
}