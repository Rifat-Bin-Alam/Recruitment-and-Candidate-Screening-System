using Microsoft.AspNetCore.Identity;

namespace RecruitmentAndCandidateScreeningSystem.Models;

public class ApplicationUser : IdentityUser
{
    public CandidateProfile? CandidateProfile { get; set; }
}