namespace RecruitmentAndCandidateScreeningSystem.ViewModels;

public class CandidateApplicationViewModel
{
    public int Id { get; set; }

    public string JobTitle { get; set; } = string.Empty;

    public string ApplicationReferenceId { get; set; } = string.Empty;

    public DateTime AppliedAt { get; set; }

    public string ApplicationStatus { get; set; } = string.Empty;

    public bool HasPayment { get; set; }

    public string PaymentStatus { get; set; } = "Not Required";

    public decimal? PaymentAmount { get; set; }

    public bool CanPay { get; set; }
}