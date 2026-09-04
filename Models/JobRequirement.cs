using System.ComponentModel.DataAnnotations;

namespace RecruitmentAndCandidateScreeningSystem.Models;

public class JobRequirement
{
    public int Id { get; set; }

    public int JobCircularId { get; set; }

    [Required]
    public string RequiredQualifications { get; set; } = string.Empty;

    public string? RequiredSkills { get; set; }

    public string? RequiredExperience { get; set; }

    public string? AdditionalRequirements { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public JobCircular JobCircular { get; set; } = null!;
}