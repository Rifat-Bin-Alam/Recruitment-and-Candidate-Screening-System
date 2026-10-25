using System.Text.RegularExpressions;
using RecruitmentAndCandidateScreeningSystem.Models;

namespace RecruitmentAndCandidateScreeningSystem.Services;

public class AIMatchingService
{
    private readonly IConfiguration? _configuration;

    public AIMatchingService(IConfiguration? configuration = null)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Local rule-based screening engine is always available without external API keys or cloud dependencies.
    /// </summary>
    public bool IsAvailable()
    {
        return true;
    }

    /// <summary>
    /// Evaluates candidate CV text against job requirements deterministically across 4 categories:
    /// 1. Required Technical Skills (45%)
    /// 2. Education & Qualifications (25%)
    /// 3. Experience Duration & Seniority (20%)
    /// 4. Preferred Criteria & Breadth (10%)
    /// </summary>
    public async Task<AIMatchingEvaluation> EvaluateAsync(
        string cvText,
        JobRequirement requirement)
    {
        if (string.IsNullOrWhiteSpace(cvText))
        {
            throw new ArgumentException("CV text is empty.", nameof(cvText));
        }

        if (requirement == null)
        {
            throw new ArgumentNullException(nameof(requirement), "Job requirement is required.");
        }

        // Run synchronously/asynchronously without external latency
        await Task.CompletedTask;

        var normalizedCv = cvText.ToLowerInvariant();

        // ------------------------------------------------------------
        // 1. REQUIRED TECHNICAL SKILLS (Weight: 45 pts)
        // ------------------------------------------------------------
        var requiredSkillsList = ParseSkillTokens(requirement.RequiredSkills);
        var matchedSkills = new List<string>();
        var missingSkills = new List<string>();

        decimal scoreSkills = 45m;
        if (requiredSkillsList.Count > 0)
        {
            foreach (var skill in requiredSkillsList)
            {
                if (IsSkillPresentInText(normalizedCv, skill))
                {
                    matchedSkills.Add(skill);
                }
                else
                {
                    missingSkills.Add(skill);
                }
            }

            decimal skillRatio = (decimal)matchedSkills.Count / requiredSkillsList.Count;
            scoreSkills = Math.Round(skillRatio * 45m, 2);
        }

        // ------------------------------------------------------------
        // 2. REQUIRED QUALIFICATIONS & ACADEMIC LEVEL (Weight: 25 pts)
        // ------------------------------------------------------------
        int targetDegreeLevel = DetermineRequiredDegreeLevel(requirement.RequiredQualifications);
        int detectedDegreeLevel = DetectHighestDegreeLevel(normalizedCv);
        bool hasFieldMatch = HasDisciplineOrFieldMatch(normalizedCv, requirement.RequiredQualifications);

        decimal scoreQualifications = 0m;
        string qualExplanation;

        if (targetDegreeLevel <= 0)
        {
            // No strict degree requirement specified
            scoreQualifications = 25m;
            qualExplanation = "No strict academic degree level required (25/25 pts)";
        }
        else if (detectedDegreeLevel >= targetDegreeLevel)
        {
            // Meets or exceeds required degree
            scoreQualifications = hasFieldMatch ? 25m : 22m;
            var fieldNote = hasFieldMatch ? "with relevant discipline/major match" : "discipline inferred from coursework";
            qualExplanation = $"Meets required {DegreeLevelToName(targetDegreeLevel)} (Detected: {DegreeLevelToName(detectedDegreeLevel)}, {fieldNote}) ({scoreQualifications}/25 pts)";
        }
        else if (detectedDegreeLevel == targetDegreeLevel - 1)
        {
            // One tier below (e.g. Diploma vs Bachelor's)
            scoreQualifications = 12m;
            qualExplanation = $"Partial qualification: Detected {DegreeLevelToName(detectedDegreeLevel)} (Target: {DegreeLevelToName(targetDegreeLevel)}) (12/25 pts)";
        }
        else
        {
            scoreQualifications = 5m;
            qualExplanation = $"Qualification below benchmark: Detected {DegreeLevelToName(detectedDegreeLevel)} (Target: {DegreeLevelToName(targetDegreeLevel)}) (5/25 pts)";
        }

        // ------------------------------------------------------------
        // 3. EXPERIENCE DURATION & SENIORITY (Weight: 20 pts)
        // ------------------------------------------------------------
        int requiredYears = ExtractRequiredYears(requirement.RequiredExperience);
        int detectedYears = DetectExperienceYearsInCv(normalizedCv);

        decimal scoreExperience = 0m;
        string expExplanation;

        if (requiredYears <= 0)
        {
            scoreExperience = 20m;
            expExplanation = $"Entry-level / no minimum years specified (Detected: ~{detectedYears} yrs) (20/20 pts)";
        }
        else if (detectedYears >= requiredYears)
        {
            scoreExperience = 20m;
            expExplanation = $"Meets experience target: ~{detectedYears} yrs detected (Required: {requiredYears} yrs) (20/20 pts)";
        }
        else
        {
            decimal expRatio = (decimal)Math.Max(0, detectedYears) / requiredYears;
            scoreExperience = Math.Round(expRatio * 20m, 2);
            expExplanation = $"Partial experience: ~{detectedYears} yrs detected (Required: {requiredYears} yrs) ({scoreExperience}/20 pts)";
        }

        // ------------------------------------------------------------
        // 4. PREFERRED CRITERIA & ADDITIONAL REQUIREMENTS (Weight: 10 pts)
        // ------------------------------------------------------------
        var preferredList = ParseSkillTokens(requirement.AdditionalRequirements);
        var matchedPreferred = new List<string>();
        decimal scorePreferred = 10m;
        string prefExplanation;

        if (preferredList.Count > 0)
        {
            foreach (var pref in preferredList)
            {
                if (IsSkillPresentInText(normalizedCv, pref))
                {
                    matchedPreferred.Add(pref);
                }
            }

            decimal prefRatio = (decimal)matchedPreferred.Count / preferredList.Count;
            scorePreferred = Math.Round(prefRatio * 10m, 2);
            prefExplanation = $"{matchedPreferred.Count} of {preferredList.Count} preferred criteria matched ({scorePreferred}/10 pts)";
        }
        else
        {
            prefExplanation = "No secondary or preferred criteria specified (10/10 pts)";
        }

        // ------------------------------------------------------------
        // 5. COMPOSITE FINAL SCORE & EXPLAINABLE AUDIT
        // ------------------------------------------------------------
        decimal totalScore = Math.Clamp(scoreSkills + scoreQualifications + scoreExperience + scorePreferred, 0m, 100m);
        totalScore = Math.Round(totalScore, 2);

        string decision = totalScore >= 80m
            ? "SHORTLISTED"
            : totalScore >= 60m
                ? "MANUAL REVIEW"
                : "REJECTED";

        var allMatchedPills = matchedSkills.Concat(matchedPreferred).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var matchedSkillsString = string.Join(", ", allMatchedPills);

        var detailsBuilder = new System.Text.StringBuilder();
        detailsBuilder.AppendLine($"DETERMINISTIC EVALUATION BREAKDOWN (Score: {totalScore:0.##}% — {decision})");
        detailsBuilder.AppendLine(new string('─', 54));
        detailsBuilder.AppendLine($"• Technical Skills ({scoreSkills}/45 pts):");
        if (requiredSkillsList.Count > 0)
        {
            detailsBuilder.AppendLine($"  - Matched Required: {(matchedSkills.Count > 0 ? string.Join(", ", matchedSkills) : "None")}");
            if (missingSkills.Count > 0)
            {
                detailsBuilder.AppendLine($"  - Missing Required: {string.Join(", ", missingSkills)}");
            }
        }
        else
        {
            detailsBuilder.AppendLine("  - No specific mandatory skills required.");
        }

        detailsBuilder.AppendLine($"• Qualifications ({scoreQualifications}/25 pts):");
        detailsBuilder.AppendLine($"  - {qualExplanation}");

        detailsBuilder.AppendLine($"• Professional Experience ({scoreExperience}/20 pts):");
        detailsBuilder.AppendLine($"  - {expExplanation}");

        detailsBuilder.AppendLine($"• Preferred & Additional ({scorePreferred}/10 pts):");
        detailsBuilder.AppendLine($"  - {prefExplanation}");
        if (matchedPreferred.Count > 0)
        {
            detailsBuilder.AppendLine($"  - Matched: {string.Join(", ", matchedPreferred)}");
        }

        detailsBuilder.AppendLine(new string('─', 54));
        detailsBuilder.AppendLine("Notice: Evaluation computed using transparent rule-based benchmarking against vacancy requirements. Score is a decision aid for the recruitment committee.");

        return new AIMatchingEvaluation
        {
            Score = totalScore,
            MatchedSkills = matchedSkillsString,
            MatchingDetails = detailsBuilder.ToString()
        };
    }

    #region Helper Matching Utilities

    private static List<string> ParseSkillTokens(string? skillsText)
    {
        if (string.IsNullOrWhiteSpace(skillsText))
            return new List<string>();

        return skillsText
            .Split(new[] { ',', ';', '\n', '\r', '/', '•', '|' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .Where(s => s.Length >= 2 && !string.Equals(s, "not specified", StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static bool IsSkillPresentInText(string normalizedText, string skill)
    {
        var cleanSkill = skill.Trim().ToLowerInvariant();

        // Skill synonyms and regular expressions
        var pattern = cleanSkill switch
        {
            "c#" => @"\b(?:c#|csharp|c-sharp)\b",
            ".net" or "dotnet" => @"\b(?:\.net|dotnet|asp\.net)\b",
            "asp.net" or "asp.net core" => @"\b(?:asp\.net|aspnet|asp\.net\s*core)\b",
            "sql" => @"\b(?:sql|tsql|plsql|sqlite|postgres|postgresql|mysql|mssql)\b",
            "javascript" or "js" => @"\b(?:javascript|js|es6|typescript|ts)\b",
            "typescript" or "ts" => @"\b(?:typescript|ts)\b",
            "react" or "react.js" => @"\b(?:react|reactjs|react\.js)\b",
            "angular" => @"\b(?:angular|angularjs)\b",
            "vue" or "vue.js" => @"\b(?:vue|vuejs|vue\.js)\b",
            "node" or "node.js" => @"\b(?:node|nodejs|node\.js)\b",
            "docker" => @"\b(?:docker|containerization|containers)\b",
            "kubernetes" or "k8s" => @"\b(?:kubernetes|k8s)\b",
            "python" => @"\b(?:python|py)\b",
            "java" => @"\b(?:java|spring|springboot|spring\s*boot)\b",
            "git" => @"\b(?:git|github|gitlab|version\s*control)\b",
            "azure" => @"\b(?:azure|microsoft\s*cloud)\b",
            "aws" => @"\b(?:aws|amazon\s*web\s*services)\b",
            "entity framework" or "ef core" => @"\b(?:entity\s*framework|ef|ef\s*core)\b",
            "rest api" or "rest" or "api" => @"\b(?:rest|restful|api|web\s*api)\b",
            "html" => @"\b(?:html|html5)\b",
            "css" => @"\b(?:css|css3|bootstrap|tailwind)\b",
            _ => $@"\b{Regex.Escape(cleanSkill)}\b"
        };

        try
        {
            return Regex.IsMatch(normalizedText, pattern, RegexOptions.IgnoreCase);
        }
        catch
        {
            return normalizedText.Contains(cleanSkill, StringComparison.OrdinalIgnoreCase);
        }
    }

    private static int DetermineRequiredDegreeLevel(string? qualificationsText)
    {
        if (string.IsNullOrWhiteSpace(qualificationsText))
            return 3; // Default to Bachelor's if not specified

        var text = qualificationsText.ToLowerInvariant();
        if (Regex.IsMatch(text, @"\b(?:ph\.?d|doctorate|doctor of philosophy)\b"))
            return 5;
        if (Regex.IsMatch(text, @"\b(?:m\.?sc|master'?s|mba|m\.?tech|m\.?s|post\s*graduate)\b"))
            return 4;
        if (Regex.IsMatch(text, @"\b(?:b\.?sc|bachelor'?s|b\.?tech|b\.?e|bba|undergraduate|degree|graduate)\b"))
            return 3;
        if (Regex.IsMatch(text, @"\b(?:diploma|associate'?s|polytechnic)\b"))
            return 2;
        if (Regex.IsMatch(text, @"\b(?:hsc|ssc|high\s*school)\b"))
            return 1;

        return 3;
    }

    private static int DetectHighestDegreeLevel(string normalizedCv)
    {
        if (Regex.IsMatch(normalizedCv, @"\b(?:ph\.?d|doctorate|doctor of philosophy)\b"))
            return 5;
        if (Regex.IsMatch(normalizedCv, @"\b(?:m\.?sc|master'?s|mba|m\.?tech|m\.?s|post\s*graduate)\b"))
            return 4;
        if (Regex.IsMatch(normalizedCv, @"\b(?:b\.?sc|bachelor'?s|b\.?tech|b\.?e|bba|b\.?a|undergraduate|degree|b\.?com)\b"))
            return 3;
        if (Regex.IsMatch(normalizedCv, @"\b(?:diploma|associate'?s|polytechnic)\b"))
            return 2;
        if (Regex.IsMatch(normalizedCv, @"\b(?:hsc|ssc|high\s*school|higher\s*secondary)\b"))
            return 1;

        return 2; // Default baseline if CV has education section
    }

    private static string DegreeLevelToName(int level) => level switch
    {
        5 => "Doctorate / PhD",
        4 => "Master's Degree",
        3 => "Bachelor's Degree",
        2 => "Diploma / Associate",
        1 => "High School",
        _ => "General Education"
    };

    private static bool HasDisciplineOrFieldMatch(string normalizedCv, string? qualifications)
    {
        var disciplines = new[]
        {
            "computer science", "software engineering", "information technology",
            "computer engineering", "cse", "cs", "it", "informatics", "electrical",
            "business", "management", "data science", "mathematics"
        };

        foreach (var discipline in disciplines)
        {
            if (normalizedCv.Contains(discipline, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    private static int ExtractRequiredYears(string? experienceText)
    {
        if (string.IsNullOrWhiteSpace(experienceText))
            return 0;

        var text = experienceText.ToLowerInvariant();
        if (text.Contains("fresh") || text.Contains("entry") || text.Contains("no experience"))
            return 0;

        var match = Regex.Match(text, @"(\d+)\+?\s*(?:year|yr)s?", RegexOptions.IgnoreCase);
        if (match.Success && int.TryParse(match.Groups[1].Value, out var years))
        {
            return years;
        }

        return 0;
    }

    private static int DetectExperienceYearsInCv(string normalizedCv)
    {
        int detectedYears = 0;

        // 1. Direct explicit mentions like "5 years of experience"
        var explicitMatches = Regex.Matches(normalizedCv, @"(\d+)\+?\s*(?:year|yr)s?(?:\s+of)?\s+(?:professional\s+)?experience", RegexOptions.IgnoreCase);
        foreach (Match match in explicitMatches)
        {
            if (int.TryParse(match.Groups[1].Value, out var yrs) && yrs > detectedYears && yrs <= 40)
            {
                detectedYears = yrs;
            }
        }

        // 2. Year ranges like "2020 - 2024" or "2019 to Present"
        var rangeMatches = Regex.Matches(normalizedCv, @"\b(20\d\d|199\d)\s*(?:-|–|to)\s*(20\d\d|present|current|now)\b", RegexOptions.IgnoreCase);
        int currentYear = DateTime.UtcNow.Year;
        int accumulatedSpan = 0;

        foreach (Match match in rangeMatches)
        {
            if (int.TryParse(match.Groups[1].Value, out var startYear))
            {
                int endYear = currentYear;
                var endStr = match.Groups[2].Value.ToLowerInvariant();
                if (endStr != "present" && endStr != "current" && endStr != "now")
                {
                    int.TryParse(endStr, out endYear);
                }

                if (endYear >= startYear && endYear <= currentYear + 1)
                {
                    accumulatedSpan += Math.Max(1, endYear - startYear);
                }
            }
        }

        if (accumulatedSpan > detectedYears && accumulatedSpan <= 40)
        {
            detectedYears = accumulatedSpan;
        }

        // If CV has a dedicated work experience section but no explicit numbers, grant a conservative 1-2 years baseline
        if (detectedYears == 0 && (normalizedCv.Contains("work experience") || normalizedCv.Contains("employment history") || normalizedCv.Contains("job experience")))
        {
            detectedYears = 2;
        }

        return detectedYears;
    }

    #endregion
}

public class AIMatchingEvaluation
{
    public decimal Score { get; set; }

    public string MatchedSkills { get; set; } = string.Empty;

    public string MatchingDetails { get; set; } = string.Empty;
}
