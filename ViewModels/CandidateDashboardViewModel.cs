namespace RecruitmentAndCandidateScreeningSystem.ViewModels;

public class CandidateDashboardViewModel
{
    public bool HasProfile { get; set; }

    public string ProfileName { get; set; } = "Candidate";

    public bool HasPhoto { get; set; }

    public bool HasSignature { get; set; }

    public int CVCount { get; set; }

    public bool HasCurrentCV { get; set; }

    public int ApplicationCount { get; set; }
}