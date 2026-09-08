using Microsoft.AspNetCore.Mvc;
using RecruitmentAndCandidateScreeningSystem.Models;
using System.Diagnostics;

namespace RecruitmentAndCandidateScreeningSystem.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            if (User.IsInRole(RoleNames.HRManager))
            {
                return RedirectToAction(
                    "Index",
                    "HRDashboard");
            }

            if (User.IsInRole(RoleNames.Candidate))
            {
                return RedirectToAction(
                    "Dashboard",
                    "Candidate");
            }
        }

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(
        Duration = 0,
        Location = ResponseCacheLocation.None,
        NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel
        {
            RequestId =
                Activity.Current?.Id ??
                HttpContext.TraceIdentifier
        });
    }
}