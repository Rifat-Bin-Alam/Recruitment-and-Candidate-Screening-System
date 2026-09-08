using System.ComponentModel.DataAnnotations;
using RecruitmentAndCandidateScreeningSystem.Models.Enums;

namespace RecruitmentAndCandidateScreeningSystem.Models;

public class Offer
{
    public int Id { get; set; }

    public int JobApplicationId { get; set; }

    [Required]
    public string OfferDetails { get; set; } = string.Empty;

    public DateTime OfferDate { get; set; } = DateTime.UtcNow;

    public DateTime? ResponseDeadline { get; set; }

    public OfferStatus Status { get; set; } = OfferStatus.Pending;

    public CandidateDecision CandidateDecision { get; set; }
        = CandidateDecision.Pending;

    public DateTime? DecisionDate { get; set; }

    public JobApplication? JobApplication { get; set; }

    public JoiningSchedule? JoiningSchedule { get; set; }
}