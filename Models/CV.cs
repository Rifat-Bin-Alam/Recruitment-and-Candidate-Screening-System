using System.ComponentModel.DataAnnotations;

namespace RecruitmentAndCandidateScreeningSystem.Models;

public class CV
{
    public int Id { get; set; }

    public int CandidateProfileId { get; set; }

    [Required]
    [StringLength(500)]
    public string FilePath { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string StoredFileName { get; set; } = string.Empty;

    [StringLength(20)]
    public string? FileExtension { get; set; }

    public long FileSize { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public bool IsCurrent { get; set; } = true;

    public CandidateProfile CandidateProfile { get; set; } = null!;

    public ICollection<JobApplication> JobApplications { get; set; } = new List<JobApplication>();
}