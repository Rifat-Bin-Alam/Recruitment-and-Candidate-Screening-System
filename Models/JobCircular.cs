using System.ComponentModel.DataAnnotations;
using RecruitmentAndCandidateScreeningSystem.Models.Enums;

namespace RecruitmentAndCandidateScreeningSystem.Models;

public class JobCircular
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string JobTitle { get; set; } = string.Empty;

    [Required]
    public string JobDescription { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Department { get; set; }

    [StringLength(200)]
    public string? Location { get; set; }

    public decimal? ApplicationFee { get; set; }

    public string Title { get; set; } = string.Empty;
    public DateTime PublishedAt { get; set; }

    public DateTime ApplicationDeadline { get; set; }

    public JobCircularStatus Status { get; set; } = JobCircularStatus.Draft;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public JobRequirement? JobRequirement { get; set; }

    public ICollection<JobApplication> JobApplications { get; set; } = new List<JobApplication>();
}