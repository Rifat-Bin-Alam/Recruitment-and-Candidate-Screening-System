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
        ".pdf", ".docx"
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

    // ============================================================
    // PROFILE
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);

        if (userId == null)
            return Challenge();

        var profile = await _context.CandidateProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(
                p => p.ApplicationUserId == userId);

        if (profile == null)
        {
            return View(new CandidateProfileViewModel());
        }

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

    // ============================================================
    // SAVE PROFILE + PHOTO + SIGNATURE + CV
    // ============================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(
        CandidateProfileViewModel model)
    {
        var userId = _userManager.GetUserId(User);

        if (userId == null)
            return Challenge();

        if (!ModelState.IsValid)
            return View(model);

        var profile = await _context.CandidateProfiles
            .Include(p => p.CVs)
            .FirstOrDefaultAsync(
                p => p.ApplicationUserId == userId);

        if (profile == null)
        {
            profile = new CandidateProfile
            {
                ApplicationUserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.CandidateProfiles.Add(profile);
        }

        // --------------------------------------------------------
        // PROFILE INFORMATION
        // --------------------------------------------------------

        profile.FullName = model.FullName;
        profile.NationalId = model.NationalId;
        profile.PhoneNumber = model.PhoneNumber;
        profile.Address = model.Address;
        profile.HighestQualification =
            model.HighestQualification;
        profile.EducationDetails =
            model.EducationDetails;
        profile.ExperienceDetails =
            model.ExperienceDetails;
        profile.Skills = model.Skills;
        profile.UpdatedAt = DateTime.UtcNow;


        // --------------------------------------------------------
        // PHOTO
        // --------------------------------------------------------

        if (model.Photo != null &&
            model.Photo.Length > 0)
        {
            var photoPath = await SaveFileAsync(
                model.Photo,
                "Photos",
                ImageExtensions,
                MaxPhotoSize);

            if (photoPath == null)
                return View(model);

            DeleteSecureFile(profile.PhotoFilePath);

            profile.PhotoFilePath = photoPath;
        }


        // --------------------------------------------------------
        // SIGNATURE
        // --------------------------------------------------------

        if (model.Signature != null &&
            model.Signature.Length > 0)
        {
            var signaturePath = await SaveFileAsync(
                model.Signature,
                "Signatures",
                ImageExtensions,
                MaxSignatureSize);

            if (signaturePath == null)
                return View(model);

            DeleteSecureFile(profile.SignatureFilePath);

            profile.SignatureFilePath = signaturePath;
        }


        // --------------------------------------------------------
        // CV
        // --------------------------------------------------------

        if (model.CV != null &&
            model.CV.Length > 0)
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
                StoredFileName =
                    Path.GetFileName(cvPath),
                FileExtension =
                    Path.GetExtension(model.CV.FileName)
                        .ToLowerInvariant(),
                FileSize = model.CV.Length,
                UploadedAt = DateTime.UtcNow,
                IsCurrent = true
            };

            _context.CVs.Add(cv);
        }


        // --------------------------------------------------------
        // SAVE EVERYTHING
        // --------------------------------------------------------

        await _context.SaveChangesAsync();

        TempData["Success"] =
            "Profile and files saved successfully.";

        return RedirectToAction(nameof(Index));
    }


    // ============================================================
    // CHANGE PHOTO
    // ============================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePhoto(
        IFormFile? photo)
    {
        if (photo == null || photo.Length == 0)
        {
            TempData["Error"] =
                "Please select a photo.";

            return RedirectToAction(nameof(Index));
        }

        var userId = _userManager.GetUserId(User);

        if (userId == null)
            return Challenge();

        var profile = await _context.CandidateProfiles
            .FirstOrDefaultAsync(
                p => p.ApplicationUserId == userId);

        if (profile == null)
        {
            TempData["Error"] =
                "Please save your profile first.";

            return RedirectToAction(nameof(Index));
        }

        var photoPath = await SaveFileAsync(
            photo,
            "Photos",
            ImageExtensions,
            MaxPhotoSize);

        if (photoPath == null)
            return RedirectToAction(nameof(Index));

        DeleteSecureFile(profile.PhotoFilePath);

        profile.PhotoFilePath = photoPath;
        profile.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        TempData["Success"] =
            "Profile photo changed successfully.";

        return RedirectToAction(nameof(Index));
    }


    // ============================================================
    // CHANGE SIGNATURE
    // ============================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeSignature(
        IFormFile? signature)
    {
        if (signature == null || signature.Length == 0)
        {
            TempData["Error"] =
                "Please select a signature.";

            return RedirectToAction(nameof(Index));
        }

        var userId = _userManager.GetUserId(User);

        if (userId == null)
            return Challenge();

        var profile = await _context.CandidateProfiles
            .FirstOrDefaultAsync(
                p => p.ApplicationUserId == userId);

        if (profile == null)
        {
            TempData["Error"] =
                "Please save your profile first.";

            return RedirectToAction(nameof(Index));
        }

        var signaturePath = await SaveFileAsync(
            signature,
            "Signatures",
            ImageExtensions,
            MaxSignatureSize);

        if (signaturePath == null)
            return RedirectToAction(nameof(Index));

        DeleteSecureFile(profile.SignatureFilePath);

        profile.SignatureFilePath = signaturePath;
        profile.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        TempData["Success"] =
            "Signature changed successfully.";

        return RedirectToAction(nameof(Index));
    }


    // ============================================================
    // VIEW PHOTO
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> Photo()
    {
        var userId = _userManager.GetUserId(User);

        if (userId == null)
            return Challenge();

        var profile = await _context.CandidateProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(
                p => p.ApplicationUserId == userId);

        if (profile == null ||
            string.IsNullOrWhiteSpace(profile.PhotoFilePath))
        {
            return NotFound();
        }

        Response.Headers.CacheControl =
            "no-store, no-cache, must-revalidate";

        Response.Headers.Pragma = "no-cache";
        Response.Headers.Expires = "0";

        return SecureFile(profile.PhotoFilePath);
    }


    // ============================================================
    // VIEW SIGNATURE
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> Signature()
    {
        var userId = _userManager.GetUserId(User);

        if (userId == null)
            return Challenge();

        var profile = await _context.CandidateProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(
                p => p.ApplicationUserId == userId);

        if (profile == null ||
            string.IsNullOrWhiteSpace(profile.SignatureFilePath))
        {
            return NotFound();
        }

        Response.Headers.CacheControl =
            "no-store, no-cache, must-revalidate";

        Response.Headers.Pragma = "no-cache";
        Response.Headers.Expires = "0";

        return SecureFile(profile.SignatureFilePath);
    }


    // ============================================================
    // SECURE FILE
    // ============================================================

    private IActionResult SecureFile(
        string relativePath)
    {
        var safeRelativePath =
            relativePath
                .Replace('/', Path.DirectorySeparatorChar)
                .Replace('\\', Path.DirectorySeparatorChar);

        var fullPath = Path.Combine(
            _environment.ContentRootPath,
            "SecureUploads",
            safeRelativePath);

        var secureRoot = Path.GetFullPath(
            Path.Combine(
                _environment.ContentRootPath,
                "SecureUploads"));

        var normalizedPath =
            Path.GetFullPath(fullPath);

        if (!normalizedPath.StartsWith(
                secureRoot,
                StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest();
        }

        if (!System.IO.File.Exists(normalizedPath))
            return NotFound();

        var extension =
            Path.GetExtension(normalizedPath)
                .ToLowerInvariant();

        var contentType = extension switch
        {
            ".jpg" => "image/jpeg",
            ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            _ => "application/octet-stream"
        };

        return PhysicalFile(
            normalizedPath,
            contentType);
    }


    // ============================================================
    // SAVE FILE
    // ============================================================

    private async Task<string?> SaveFileAsync(
        IFormFile file,
        string folder,
        string[] allowedExtensions,
        long maxSize)
    {
        var extension =
            Path.GetExtension(file.FileName)
                .ToLowerInvariant();

        if (!allowedExtensions.Contains(extension))
        {
            ModelState.AddModelError(
                string.Empty,
                $"Invalid file type for {folder}. " +
                $"Allowed types: " +
                $"{string.Join(", ", allowedExtensions)}.");

            return null;
        }

        if (file.Length <= 0 ||
            file.Length > maxSize)
        {
            ModelState.AddModelError(
                string.Empty,
                $"The {folder} file exceeds " +
                $"the allowed size.");

            return null;
        }

        var uploadRoot = Path.Combine(
            _environment.ContentRootPath,
            "SecureUploads",
            folder);

        Directory.CreateDirectory(uploadRoot);

        var storedFileName =
            $"{Guid.NewGuid():N}{extension}";

        var fullPath = Path.Combine(
            uploadRoot,
            storedFileName);

        await using var stream =
            new FileStream(
                fullPath,
                FileMode.CreateNew);

        await file.CopyToAsync(stream);

        return Path.Combine(
            folder,
            storedFileName);
    }


    // ============================================================
    // DELETE OLD FILE
    // ============================================================

    private void DeleteSecureFile(
        string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            return;

        var fullPath = Path.Combine(
            _environment.ContentRootPath,
            "SecureUploads",
            relativePath);

        if (System.IO.File.Exists(fullPath))
        {
            try
            {
                System.IO.File.Delete(fullPath);
            }
            catch
            {
                // Do not stop profile update
                // if old file cannot be deleted.
            }
        }
    }
}