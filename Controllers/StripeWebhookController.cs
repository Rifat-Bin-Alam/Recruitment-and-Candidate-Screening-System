using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecruitmentAndCandidateScreeningSystem.Data;
using RecruitmentAndCandidateScreeningSystem.Models.Enums;
using Stripe;

namespace RecruitmentAndCandidateScreeningSystem.Controllers;

[ApiController]
[Route("api/stripe")]
public class StripeWebhookController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public StripeWebhookController(
        ApplicationDbContext context,
        IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> Webhook()
    {
        var json = await new StreamReader(Request.Body).ReadToEndAsync();

        var webhookSecret =
            _configuration["Stripe:WebhookSecret"];

        if (string.IsNullOrWhiteSpace(webhookSecret))
            return BadRequest("Stripe webhook secret is not configured.");

        Stripe.Event stripeEvent;

        try
        {
            stripeEvent = EventUtility.ConstructEvent(
                json,
                Request.Headers["Stripe-Signature"],
                webhookSecret);
        }
        catch
        {
            return BadRequest();
        }

        if (stripeEvent.Type == EventTypes.CheckoutSessionCompleted)
        {
            var session = stripeEvent.Data.Object
                as Stripe.Checkout.Session;

            if (session == null)
                return BadRequest();

            if (session.PaymentStatus != "paid")
                return Ok();

            var applicationReferenceId =
                session.Metadata.TryGetValue(
                    "application_reference_id",
                    out var referenceId)
                    ? referenceId
                    : session.ClientReferenceId;

            if (string.IsNullOrWhiteSpace(applicationReferenceId))
                return BadRequest();

            var application = await _context.JobApplications
                .Include(a => a.Payment)
                .FirstOrDefaultAsync(a =>
                    a.ApplicationReferenceId ==
                    applicationReferenceId);

            if (application == null)
                return NotFound();

            if (application.Payment != null)
            {
                application.Payment.Status =
                    PaymentStatus.Paid;

                application.Payment.PaidAt =
                    DateTime.UtcNow;

                application.Payment.TransactionId =
                    session.PaymentIntentId;
            }

            await _context.SaveChangesAsync();
        }

        return Ok();
    }
}