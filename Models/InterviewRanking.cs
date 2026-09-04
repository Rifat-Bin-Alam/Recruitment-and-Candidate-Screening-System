using System.ComponentModel.DataAnnotations;

namespace RecruitmentAndCandidateScreeningSystem.Models;

public class InterviewRanking
{
    public int Id { get; set; }

    public int JobApplicationId { get; set; }

    [Range(0, 100)]
    public decimal InterviewScore { get; set; }

    public int Ranking { get; set; }

    [StringLength(2000)]
    public string? Remarks { get; set; }

    public DateTime InterviewDate { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public JobApplication JobApplication { get; set; } = null!;
}