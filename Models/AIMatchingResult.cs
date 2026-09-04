using System.ComponentModel.DataAnnotations;

namespace RecruitmentAndCandidateScreeningSystem.Models;

public class AIMatchingResult
{
    public int Id { get; set; }

    public int JobApplicationId { get; set; }

    [Range(0, 100)]
    public decimal MatchingScore { get; set; }

    [Required]
    [StringLength(50)]
    public string Decision { get; set; } = string.Empty;

    public string? MatchedSkills { get; set; }

    public string? MatchingDetails { get; set; }

    public DateTime EvaluatedAt { get; set; } = DateTime.UtcNow;

    public JobApplication JobApplication { get; set; } = null!;
}