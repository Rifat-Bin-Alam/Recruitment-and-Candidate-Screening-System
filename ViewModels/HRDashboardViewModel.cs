namespace RecruitmentAndCandidateScreeningSystem.ViewModels;

public class HRDashboardViewModel
{
    public int TotalApplications { get; set; }

    public int AIEvaluated { get; set; }

    public int Shortlisted { get; set; }

    public int ManualReview { get; set; }

    public int Rejected { get; set; }

    public int PaymentPending { get; set; }

    public int PublishedJobs { get; set; }

    public List<RecentApplicationViewModel> RecentApplications { get; set; }
        = new();
}

public class RecentApplicationViewModel
{
    public int Id { get; set; }

    public string ApplicationReferenceId { get; set; }
        = string.Empty;

    public string CandidateName { get; set; }
        = string.Empty;

    public string JobTitle { get; set; }
        = string.Empty;

    public DateTime AppliedAt { get; set; }

    public string ApplicationStatus { get; set; }
        = string.Empty;

    public decimal? AIMatchingScore { get; set; }

    public string? AIDecision { get; set; }
}