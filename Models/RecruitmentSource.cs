using System.ComponentModel.DataAnnotations;

namespace RecruitmentAndCandidateScreeningSystem.Models;

public class RecruitmentSource
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    public ICollection<JobApplication> JobApplications { get; set; } = new List<JobApplication>();
}