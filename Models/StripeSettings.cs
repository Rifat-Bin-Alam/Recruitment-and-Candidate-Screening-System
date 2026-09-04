namespace RecruitmentAndCandidateScreeningSystem.Models;

public class StripeSettings
{
    public string SecretKey { get; set; } = string.Empty;

    public string Currency { get; set; } = "usd";
}