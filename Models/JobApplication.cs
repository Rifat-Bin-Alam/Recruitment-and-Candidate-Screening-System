using System.ComponentModel.DataAnnotations;
using RecruitmentAndCandidateScreeningSystem.Models.Enums;

namespace RecruitmentAndCandidateScreeningSystem.Models;

public class JobApplication
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string ApplicationReferenceId { get; set; } = string.Empty;

    public int CandidateProfileId { get; set; }

    public int JobCircularId { get; set; }

    public int CVId { get; set; }

    public int? RecruitmentSourceId { get; set; }

    public JobApplicationStatus Status { get; set; } = JobApplicationStatus.Submitted;

    public DateTime AppliedAt { get; set; } = DateTime.UtcNow;

    public CandidateProfile CandidateProfile { get; set; } = null!;

    public JobCircular JobCircular { get; set; } = null!;

    public CV CV { get; set; } = null!;

    public RecruitmentSource? RecruitmentSource { get; set; }

    public Payment? Payment { get; set; }

    public AIMatchingResult? AIMatchingResult { get; set; }

    public InterviewRanking? InterviewRanking { get; set; }

    public Offer? Offer { get; set; }
}