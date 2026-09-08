using Google.GenAI;
using Google.GenAI.Types;

namespace RecruitmentAndCandidateScreeningSystem.Services;

public class GeminiService
{
    private readonly IConfiguration _configuration;

    public GeminiService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<string> TestAsync()
    {
        var apiKey = _configuration["Gemini:ApiKey"];
        var modelName =
            _configuration["Gemini:Model"] ?? "gemini-3.7-flash";

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException(
                "Gemini API key is not configured.");
        }

        var client = new Client(
            apiKey: apiKey);

        var response = await client.Models.GenerateContentAsync(
            model: modelName,
            contents: "Reply with exactly: Gemini connection successful.");

        return response.Text ?? string.Empty;
    }
}