using System.ComponentModel.DataAnnotations;

namespace RecruitmentAndCandidateScreeningSystem.Models;

public class JoiningSchedule
{
    public int Id { get; set; }

    public int OfferId { get; set; }

    public DateTime? SelectedJoiningDate { get; set; }

    public DateTime? ConfirmedJoiningDate { get; set; }

    public bool IsConfirmed { get; set; }

    public DateTime? ConfirmedAt { get; set; }

    public Offer Offer { get; set; } = null!;
}