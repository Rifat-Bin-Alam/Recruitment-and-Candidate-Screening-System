using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecruitmentAndCandidateScreeningSystem.Data;
using RecruitmentAndCandidateScreeningSystem.Models;
using RecruitmentAndCandidateScreeningSystem.Models.Enums;
using RecruitmentAndCandidateScreeningSystem.ViewModels;

namespace RecruitmentAndCandidateScreeningSystem.Controllers;

[Authorize(Roles = RoleNames.HRManager)]
public class HRApplicationsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public HRApplicationsController(
        ApplicationDbContext context,
        IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    public async Task<IActionResult> Index(
        string? search,
        string? status)
    {
        var applicationsQuery =
            _context.JobApplications
                .Include(a => a.CandidateProfile)
                    .ThenInclude(p => p.ApplicationUser)
                .Include(a => a.JobCircular)
                .Include(a => a.Payment)
                .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();

            applicationsQuery =
                applicationsQuery.Where(a =>
                    a.CandidateProfile.FullName.Contains(search) ||
                    a.CandidateProfile.ApplicationUser.Email!
                        .Contains(search) ||
                    a.JobCircular!.Title.Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            if (Enum.TryParse<JobApplicationStatus>(
                    status,
                    true,
                    out var selectedStatus))
            {
                applicationsQuery =
                    applicationsQuery.Where(a =>
                        a.Status == selectedStatus);
            }
        }

        var applications =
            await applicationsQuery
                .OrderByDescending(a => a.AppliedAt)
                .Select(a => new HRApplicationViewModel
                {
                    Id = a.Id,

                    ApplicationReferenceId =
                        a.Payment != null &&
                        a.Payment.Status != PaymentStatus.Paid
                            ? string.Empty
                            : a.ApplicationReferenceId,

                    CandidateName =
                        a.CandidateProfile.FullName,

                    CandidateEmail =
                        a.CandidateProfile.ApplicationUser.Email
                        ?? string.Empty,

                    JobTitle =
                        a.JobCircular!.Title,

                    AppliedAt =
                        a.AppliedAt,

                    ApplicationStatus =
                        a.Payment != null &&
                        a.Payment.Status != PaymentStatus.Paid
                            ? "Payment Pending"
                            : a.Status.ToString(),

                    PaymentStatus =
                        a.Payment == null
                            ? "Not Required"
                            : a.Payment.Status.ToString(),

                    PaymentAmount =
                        a.Payment == null
                            ? null
                            : a.Payment.Amount
                })
                .ToListAsync();

        ViewBag.Search = search;
        ViewBag.Status = status;

        return View(
            "~/Views/HRApplications/Index.cshtml",
            applications);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
            return NotFound();

        var application =
            await _context.JobApplications
                .Include(a => a.CandidateProfile)
                    .ThenInclude(p => p.ApplicationUser)
                .Include(a => a.CandidateProfile)
                    .ThenInclude(p => p.CVs)
                .Include(a => a.JobCircular)
                    .ThenInclude(j => j.JobRequirement)
                .Include(a => a.Payment)
                .FirstOrDefaultAsync(a => a.Id == id);

        if (application == null)
            return NotFound();

        return View(
            "~/Views/HRApplications/Details.cshtml",
            application);
    }

    [HttpGet]
    public async Task<IActionResult> CandidateFile(
        int applicationId,
        string type)
    {
        var application =
            await _context.JobApplications
                .Include(a => a.CandidateProfile)
                    .ThenInclude(p => p.CVs)
                .FirstOrDefaultAsync(a =>
                    a.Id == applicationId);

        if (application == null ||
            application.CandidateProfile == null)
        {
            return NotFound();
        }

        string? relativePath = null;

        switch (type.ToLowerInvariant())
        {
            case "photo":
                relativePath =
                    application.CandidateProfile.PhotoFilePath;
                break;

            case "signature":
                relativePath =
                    application.CandidateProfile.SignatureFilePath;
                break;

            case "cv":
                var currentCV =
                    application.CandidateProfile.CVs
                        .FirstOrDefault(c => c.IsCurrent);

                relativePath =
                    currentCV?.FilePath;
                break;

            default:
                return BadRequest();
        }

        if (string.IsNullOrWhiteSpace(relativePath))
            return NotFound();

        var secureRoot =
            Path.GetFullPath(
                Path.Combine(
                    _environment.ContentRootPath,
                    "SecureUploads"));

        var fullPath =
            Path.GetFullPath(
                Path.Combine(
                    secureRoot,
                    relativePath));

        if (!fullPath.StartsWith(
                secureRoot + Path.DirectorySeparatorChar,
                StringComparison.OrdinalIgnoreCase))
        {
            return Forbid();
        }

        if (!System.IO.File.Exists(fullPath))
            return NotFound();

        var extension =
            Path.GetExtension(fullPath)
                .ToLowerInvariant();

        var contentType = extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" =>
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            _ => "application/octet-stream"
        };

        return PhysicalFile(
            fullPath,
            contentType);
    }
}