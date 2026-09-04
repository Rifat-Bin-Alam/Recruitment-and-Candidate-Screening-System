using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecruitmentAndCandidateScreeningSystem.Data;
using RecruitmentAndCandidateScreeningSystem.Models;
using RecruitmentAndCandidateScreeningSystem.Models.Enums;

namespace RecruitmentAndCandidateScreeningSystem.Controllers;

[Authorize(Roles = RoleNames.HRManager)]
public class JobCircularsController : Controller
{
    private readonly ApplicationDbContext _context;

    public JobCircularsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var jobs = await _context.JobCirculars
            .Include(j => j.JobRequirement)
            .OrderByDescending(j => j.CreatedAt)
            .ToListAsync();

        return View(jobs);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(JobCircular jobCircular)
    {
        if (!ModelState.IsValid)
        {
            return View(jobCircular);
        }

        jobCircular.Status = JobCircularStatus.Draft;
        jobCircular.CreatedAt = DateTime.UtcNow;

        _context.JobCirculars.Add(jobCircular);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
            return NotFound();

        var job = await _context.JobCirculars
            .Include(j => j.JobRequirement)
            .FirstOrDefaultAsync(j => j.Id == id);

        if (job == null)
            return NotFound();

        return View(job);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
            return NotFound();

        var job = await _context.JobCirculars
            .FirstOrDefaultAsync(j => j.Id == id);

        if (job == null)
            return NotFound();

        return View(job);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, JobCircular jobCircular)
    {
        if (id != jobCircular.Id)
            return NotFound();

        if (!ModelState.IsValid)
            return View(jobCircular);

        var existingJob = await _context.JobCirculars
            .FirstOrDefaultAsync(j => j.Id == id);

        if (existingJob == null)
            return NotFound();

        existingJob.JobTitle = jobCircular.JobTitle;
        existingJob.JobDescription = jobCircular.JobDescription;
        existingJob.Department = jobCircular.Department;
        existingJob.Location = jobCircular.Location;
        existingJob.ApplicationFee = jobCircular.ApplicationFee;
        existingJob.PublishedAt = jobCircular.PublishedAt;
        existingJob.ApplicationDeadline = jobCircular.ApplicationDeadline;
        existingJob.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Publish(int id)
    {
        var job = await _context.JobCirculars.FindAsync(id);

        if (job == null)
            return NotFound();

        if (job.ApplicationDeadline <= DateTime.UtcNow)
        {
            TempData["Error"] = "The application deadline must be in the future.";
            return RedirectToAction(nameof(Index));
        }

        job.Status = JobCircularStatus.Published;
        job.PublishedAt = DateTime.UtcNow;
        job.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Close(int id)
    {
        var job = await _context.JobCirculars.FindAsync(id);

        if (job == null)
            return NotFound();

        job.Status = JobCircularStatus.Closed;
        job.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveRequirements(
     int jobCircularId,
     string requiredQualifications,
     string? requiredSkills,
     string? requiredExperience,
     string? additionalRequirements)
    {
        var job = await _context.JobCirculars
            .Include(j => j.JobRequirement)
            .FirstOrDefaultAsync(j => j.Id == jobCircularId);

        if (job == null)
            return NotFound();

        if (job.JobRequirement == null)
        {
            var requirement = new JobRequirement
            {
                JobCircularId = jobCircularId,
                RequiredQualifications = requiredQualifications,
                RequiredSkills = requiredSkills,
                RequiredExperience = requiredExperience,
                AdditionalRequirements = additionalRequirements,
                CreatedAt = DateTime.UtcNow
            };

            _context.JobRequirements.Add(requirement);
        }
        else
        {
            job.JobRequirement.RequiredQualifications = requiredQualifications;
            job.JobRequirement.RequiredSkills = requiredSkills;
            job.JobRequirement.RequiredExperience = requiredExperience;
            job.JobRequirement.AdditionalRequirements = additionalRequirements;
            job.JobRequirement.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = jobCircularId });
    }
}