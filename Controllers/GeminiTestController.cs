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
            if (!_geminiService.IsAvailable())
            {
                return Content("Integration Notice\n\nThis demonstration version has certain external API integrations disabled for deployment purposes.\n\nFor access to the full functionality, including payment processing and AI-powered CV matching, please contact me by email: rifat5566123@gmail.com");
            }

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