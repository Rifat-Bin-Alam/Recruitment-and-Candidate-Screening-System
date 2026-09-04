namespace RecruitmentAndCandidateScreeningSystem.ViewModels;

public class HRApplicationViewModel
{
    public int Id { get; set; }

    public string ApplicationReferenceId { get; set; } = string.Empty;

    public string CandidateName { get; set; } = string.Empty;

    public string CandidateEmail { get; set; } = string.Empty;

    public string JobTitle { get; set; } = string.Empty;

    public DateTime AppliedAt { get; set; }

    public string ApplicationStatus { get; set; } = string.Empty;

    public string PaymentStatus { get; set; } = "Not Required";

    public decimal? PaymentAmount { get; set; }
}