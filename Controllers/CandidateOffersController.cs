using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecruitmentAndCandidateScreeningSystem.Data;
using RecruitmentAndCandidateScreeningSystem.Models;
using RecruitmentAndCandidateScreeningSystem.Models.Enums;

namespace RecruitmentAndCandidateScreeningSystem.Controllers;

[Authorize(Roles = RoleNames.Candidate)]
public class CandidateOffersController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public CandidateOffersController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // Candidate's offers
    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);

        if (userId == null)
            return Challenge();

        var profile = await _context.CandidateProfiles
            .FirstOrDefaultAsync(p =>
                p.ApplicationUserId == userId);

        if (profile == null)
        {
            return NotFound();
        }

        var offers = await _context.Offers
            .AsNoTracking()
            .Include(o => o.JobApplication)
                .ThenInclude(a => a.JobCircular)
            .Include(o => o.JoiningSchedule)
            .Where(o =>
                o.JobApplication.CandidateProfileId == profile.Id)
            .OrderByDescending(o => o.OfferDate)
            .ToListAsync();

        return View(offers);
    }

    // Candidate views an offer
    public async Task<IActionResult> Details(int id)
    {
        var userId = _userManager.GetUserId(User);

        if (userId == null)
            return Challenge();

        var profile = await _context.CandidateProfiles
            .FirstOrDefaultAsync(p =>
                p.ApplicationUserId == userId);

        if (profile == null)
        {
            return NotFound();
        }

        var offer = await _context.Offers
            .Include(o => o.JobApplication)
                .ThenInclude(a => a.JobCircular)
            .Include(o => o.JoiningSchedule)
            .FirstOrDefaultAsync(o =>
                o.Id == id &&
                o.JobApplication.CandidateProfileId == profile.Id);

        if (offer == null)
        {
            return NotFound();
        }

        return View(offer);
    }

    // Candidate accepts offer
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Accept(int id)
    {
        var userId = _userManager.GetUserId(User);

        if (userId == null)
            return Challenge();

        var profile = await _context.CandidateProfiles
            .FirstOrDefaultAsync(p =>
                p.ApplicationUserId == userId);

        if (profile == null)
            return NotFound();

        var offer = await _context.Offers
            .Include(o => o.JobApplication)
            .Include(o => o.JoiningSchedule)
            .FirstOrDefaultAsync(o =>
                o.Id == id &&
                o.JobApplication.CandidateProfileId == profile.Id);

        if (offer == null)
            return NotFound();

        if (offer.Status != OfferStatus.Pending ||
            offer.CandidateDecision != CandidateDecision.Pending)
        {
            TempData["Error"] =
                "This offer has already been processed.";

            return RedirectToAction(nameof(Details), new { id });
        }

        if (offer.ResponseDeadline.HasValue &&
            offer.ResponseDeadline.Value < DateTime.UtcNow)
        {
            TempData["Error"] =
                "The response deadline for this offer has passed.";

            return RedirectToAction(nameof(Details), new { id });
        }

        // Accept the offer
        offer.Status = OfferStatus.Accepted;
        offer.CandidateDecision = CandidateDecision.Accepted;
        offer.DecisionDate = DateTime.UtcNow;

        // Confirm the joining schedule provided by HR
        if (offer.JoiningSchedule != null &&
            offer.JoiningSchedule.SelectedJoiningDate.HasValue)
        {
            offer.JoiningSchedule.ConfirmedJoiningDate =
                offer.JoiningSchedule.SelectedJoiningDate;

            offer.JoiningSchedule.IsConfirmed = true;
            offer.JoiningSchedule.ConfirmedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        TempData["Success"] =
            "Offer accepted and joining schedule confirmed successfully.";

        return RedirectToAction(nameof(Details), new { id });
    }

    // Candidate rejects offer
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id)
    {
        var userId = _userManager.GetUserId(User);

        if (userId == null)
            return Challenge();

        var profile = await _context.CandidateProfiles
            .FirstOrDefaultAsync(p =>
                p.ApplicationUserId == userId);

        if (profile == null)
        {
            return NotFound();
        }

        var offer = await _context.Offers
            .Include(o => o.JobApplication)
            .FirstOrDefaultAsync(o =>
                o.Id == id &&
                o.JobApplication.CandidateProfileId == profile.Id);

        if (offer == null)
        {
            return NotFound();
        }

        if (offer.Status != OfferStatus.Pending ||
            offer.CandidateDecision != CandidateDecision.Pending)
        {
            TempData["Error"] =
                "This offer has already been processed.";

            return RedirectToAction(nameof(Details), new { id });
        }

        if (offer.ResponseDeadline.HasValue &&
            offer.ResponseDeadline.Value < DateTime.UtcNow)
        {
            TempData["Error"] =
                "The response deadline for this offer has passed.";

            return RedirectToAction(nameof(Details), new { id });
        }

        offer.Status = OfferStatus.Rejected;
        offer.CandidateDecision = CandidateDecision.Rejected;
        offer.DecisionDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        TempData["Success"] =
            "Offer rejected successfully.";

        return RedirectToAction(nameof(Details), new { id });
    }
}