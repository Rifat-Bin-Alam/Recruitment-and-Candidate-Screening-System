using System.Text.Json;
using Google.GenAI;
using RecruitmentAndCandidateScreeningSystem.Models;

namespace RecruitmentAndCandidateScreeningSystem.Services;

public class AIMatchingService
{
    private readonly IConfiguration _configuration;

    public AIMatchingService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<AIMatchingEvaluation> EvaluateAsync(
        string cvText,
        JobRequirement requirement)
    {
        if (string.IsNullOrWhiteSpace(cvText))
            throw new ArgumentException("CV text is empty.");

        var apiKey = _configuration["Gemini:ApiKey"];

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException(
                "Gemini API key is not configured.");
        }

        var modelName =
            _configuration["Gemini:Model"]
            ?? "gemini-3.7-flash";

        var client = new Client(apiKey: apiKey);

        var prompt = $$"""
You are an AI recruitment assistant.

Your task is to evaluate how well a candidate's CV matches a specific job requirement.

IMPORTANT FAIRNESS RULES:
- Evaluate ONLY education, skills, professional experience, and job-related qualifications.
- DO NOT consider or infer:
  - name
  - gender
  - age
  - nationality
  - religion
  - photograph
  - signature
  - national ID
  - address
  - marital status
  - any other personal or demographic information.
- Do not make assumptions about the candidate beyond information explicitly present in the CV.

JOB REQUIREMENTS:

Required Qualifications:
{{requirement.RequiredQualifications}}

Required Skills:
{{requirement.RequiredSkills ?? "Not specified"}}

Required Experience:
{{requirement.RequiredExperience ?? "Not specified"}}

Additional Requirements:
{{requirement.AdditionalRequirements ?? "Not specified"}}

CANDIDATE CV:

{{cvText}}

Evaluate the candidate against the job requirements.

Return ONLY valid JSON in exactly this structure:

{
  "score": 0,
  "matchedSkills": "skill1, skill2",
  "matchingDetails": "Brief explanation of why the candidate matches or does not match."
}

Rules for score:
- Score must be between 0 and 100.
- Consider the relevance and strength of the candidate's qualifications, skills and experience.
- Do not use personal information when calculating the score.
""";

        var response = await client.Models.GenerateContentAsync(
            model: modelName,
            contents: prompt);

        var responseText = response.Text;

        if (string.IsNullOrWhiteSpace(responseText))
        {
            throw new InvalidOperationException(
                "Gemini returned an empty response.");
        }

        responseText = CleanJson(responseText);

        var evaluation =
            JsonSerializer.Deserialize<AIMatchingEvaluation>(
                responseText,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

        if (evaluation == null)
        {
            throw new InvalidOperationException(
                "Unable to read Gemini's matching result.");
        }

        evaluation.Score =
            Math.Clamp(evaluation.Score, 0, 100);

        return evaluation;
    }

    private static string CleanJson(string response)
    {
        response = response.Trim();

        if (response.StartsWith("```"))
        {
            var firstNewLine = response.IndexOf('\n');

            if (firstNewLine >= 0)
            {
                response = response[(firstNewLine + 1)..];
            }

            if (response.EndsWith("```"))
            {
                response = response[..^3];
            }
        }

        return response.Trim();
    }
}

public class AIMatchingEvaluation
{
    public decimal Score { get; set; }

    public string MatchedSkills { get; set; } = string.Empty;

    public string MatchingDetails { get; set; } = string.Empty;
}