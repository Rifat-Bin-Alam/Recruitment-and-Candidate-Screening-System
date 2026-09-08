namespace RecruitmentAndCandidateScreeningSystem.ViewModels;

public class RecruitmentReportViewModel
{
    // Application statistics
    public int TotalApplications { get; set; }

    public int AIEvaluated { get; set; }

    public int Shortlisted { get; set; }

    public int ManualReview { get; set; }

    public int Rejected { get; set; }

    public int InterviewCandidates { get; set; }

    // Offer statistics
    public int TotalOffers { get; set; }

    public int PendingOffers { get; set; }

    public int AcceptedOffers { get; set; }

    public int RejectedOffers { get; set; }

    // Payment statistics
    public int PaidApplications { get; set; }

    public int PendingPayments { get; set; }

    // Job statistics
    public int PublishedJobs { get; set; }

    // Job-wise statistics
    public List<JobRecruitmentReportItem> JobStatistics { get; set; }
        = new();
}


public class JobRecruitmentReportItem
{
    public int JobCircularId { get; set; }

    public string JobTitle { get; set; } = string.Empty;

    public int TotalApplications { get; set; }

    public int AIEvaluated { get; set; }

    public int Shortlisted { get; set; }

    public int ManualReview { get; set; }

    public int Rejected { get; set; }

    public int InterviewCandidates { get; set; }

    public int Offers { get; set; }

    public int AcceptedOffers { get; set; }
}