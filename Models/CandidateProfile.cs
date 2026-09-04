using System.ComponentModel.DataAnnotations;

namespace RecruitmentAndCandidateScreeningSystem.Models;

public class CandidateProfile
{
    public int Id { get; set; }

    [Required]
    public string ApplicationUserId { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [StringLength(20)]
    public string? NationalId { get; set; }

    [Phone]
    public string? PhoneNumber { get; set; }

    [StringLength(500)]
    public string? Address { get; set; }

    [StringLength(100)]
    public string? HighestQualification { get; set; }

    public string? EducationDetails { get; set; }

    public string? ExperienceDetails { get; set; }

    public string? Skills { get; set; }

    [StringLength(500)]
    public string? PhotoFilePath { get; set; }

    [StringLength(500)]
    public string? SignatureFilePath { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public ApplicationUser ApplicationUser { get; set; } = null!;

    public ICollection<CV> CVs { get; set; } = new List<CV>();

    public ICollection<JobApplication> JobApplications { get; set; } = new List<JobApplication>();
}