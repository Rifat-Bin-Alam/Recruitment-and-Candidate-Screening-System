using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecruitmentAndCandidateScreeningSystem.Data;
using RecruitmentAndCandidateScreeningSystem.Models;
using RecruitmentAndCandidateScreeningSystem.ViewModels;

namespace RecruitmentAndCandidateScreeningSystem.Controllers;

[Authorize(Roles = RoleNames.Candidate)]
public class CandidateProfilesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebHostEnvironment _environment;

    private const long MaxPhotoSize = 2 * 1024 * 1024;
    private const long MaxSignatureSize = 2 * 1024 * 1024;
    private const long MaxCVSize = 5 * 1024 * 1024;

    private static readonly string[] ImageExtensions =
    {
        ".jpg", ".jpeg", ".png"
    };

    private static readonly string[] CVExtensions =
    {
        ".pdf", ".doc", ".docx"
    };

    public CandidateProfilesController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IWebHostEnvironment environment)
    {
        _context = context;
        _userManager = userManager;
        _environment = environment;
    }

    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);

        if (userId == null)
            return Challenge();

        var profile = await _context.CandidateProfiles
            .FirstOrDefaultAsync(p => p.ApplicationUserId == userId);

        if (profile == null)
            return View(new CandidateProfileViewModel());

        var model = new CandidateProfileViewModel
        {
            FullName = profile.FullName,
            NationalId = profile.NationalId,
            PhoneNumber = profile.PhoneNumber,
            Address = profile.Address,
            HighestQualification = profile.HighestQualification,
            EducationDetails = profile.EducationDetails,
            ExperienceDetails = profile.ExperienceDetails,
            Skills = profile.Skills
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(CandidateProfileViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var userId = _userManager.GetUserId(User);

        if (userId == null)
            return Challenge();

        var profile = await _context.CandidateProfiles
            .Include(p => p.CVs)
            .FirstOrDefaultAsync(p => p.ApplicationUserId == userId);

        if (profile == null)
        {
            profile = new CandidateProfile
            {
                ApplicationUserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.CandidateProfiles.Add(profile);
        }

        profile.FullName = model.FullName;
        profile.NationalId = model.NationalId;
        profile.PhoneNumber = model.PhoneNumber;
        profile.Address = model.Address;
        profile.HighestQualification = model.HighestQualification;
        profile.EducationDetails = model.EducationDetails;
        profile.ExperienceDetails = model.ExperienceDetails;
        profile.Skills = model.Skills;
        profile.UpdatedAt = DateTime.UtcNow;

        if (model.Photo != null)
        {
            var photoPath = await SaveFileAsync(
                model.Photo,
                "Photos",
                ImageExtensions,
                MaxPhotoSize);

            if (photoPath == null)
                return View(model);

            profile.PhotoFilePath = photoPath;
        }

        if (model.Signature != null)
        {
            var signaturePath = await SaveFileAsync(
                model.Signature,
                "Signatures",
                ImageExtensions,
                MaxSignatureSize);

            if (signaturePath == null)
                return View(model);

            profile.SignatureFilePath = signaturePath;
        }

        if (model.CV != null)
        {
            var cvPath = await SaveFileAsync(
                model.CV,
                "CVs",
                CVExtensions,
                MaxCVSize);

            if (cvPath == null)
                return View(model);

            foreach (var existingCV in profile.CVs)
            {
                existingCV.IsCurrent = false;
            }

            var cv = new CV
            {
                CandidateProfile = profile,
                FilePath = cvPath,
                StoredFileName = Path.GetFileName(cvPath),
                FileExtension = Path.GetExtension(model.CV.FileName).ToLowerInvariant(),
                FileSize = model.CV.Length,
                UploadedAt = DateTime.UtcNow,
                IsCurrent = true
            };

            _context.CVs.Add(cv);
        }

        await _context.SaveChangesAsync();

        TempData["Success"] = "Profile and files saved successfully.";

        return RedirectToAction(nameof(Index));
    }

    private async Task<string?> SaveFileAsync(
        IFormFile file,
        string folder,
        string[] allowedExtensions,
        long maxSize)
    {
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(extension))
        {
            ModelState.AddModelError(
                string.Empty,
                $"Invalid file type for {folder}. Allowed types: {string.Join(", ", allowedExtensions)}.");

            return null;
        }

        if (file.Length <= 0 || file.Length > maxSize)
        {
            ModelState.AddModelError(
                string.Empty,
                $"The {folder} file exceeds the allowed size.");

            return null;
        }

        var uploadRoot = Path.Combine(
            _environment.ContentRootPath,
            "SecureUploads",
            folder);

        Directory.CreateDirectory(uploadRoot);

        var storedFileName =
            $"{Guid.NewGuid():N}{extension}";

        var fullPath = Path.Combine(uploadRoot, storedFileName);

        await using var stream = new FileStream(
            fullPath,
            FileMode.CreateNew);

        await file.CopyToAsync(stream);

        return Path.Combine(folder, storedFileName);
    }
}