using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace RecruitmentAndCandidateScreeningSystem.ViewModels;

public class CandidateProfileViewModel
{
    [Required]
    [StringLength(100)]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [StringLength(20)]
    [Display(Name = "National ID")]
    public string? NationalId { get; set; }

    [Phone]
    [Display(Name = "Phone Number")]
    public string? PhoneNumber { get; set; }

    [StringLength(500)]
    public string? Address { get; set; }

    [StringLength(100)]
    [Display(Name = "Highest Qualification")]
    public string? HighestQualification { get; set; }

    [Display(Name = "Education Details")]
    public string? EducationDetails { get; set; }

    [Display(Name = "Experience Details")]
    public string? ExperienceDetails { get; set; }

    public string? Skills { get; set; }

    [Display(Name = "Profile Photo")]
    public IFormFile? Photo { get; set; }

    [Display(Name = "Signature")]
    public IFormFile? Signature { get; set; }

    [Display(Name = "CV")]
    public IFormFile? CV { get; set; }
}