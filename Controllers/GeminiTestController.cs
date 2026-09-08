using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecruitmentAndCandidateScreeningSystem.Services;

namespace RecruitmentAndCandidateScreeningSystem.Controllers;

[Authorize(Roles = "HRManager")]
public class GeminiTestController : Controller
{
    private readonly GeminiService _geminiService;

    public GeminiTestController(GeminiService geminiService)
    {
        _geminiService = geminiService;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var result = await _geminiService.TestAsync();

            return Content(result);
        }
        catch (Exception ex)
        {
            return Content(
                $"Gemini test failed:\n\n{ex.Message}");
        }
    }
}