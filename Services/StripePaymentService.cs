using Stripe.Checkout;

namespace RecruitmentAndCandidateScreeningSystem.Services;

public class StripePaymentService
{
    private readonly IConfiguration _configuration;

    public StripePaymentService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<Session> CreateCheckoutSessionAsync(
        string applicationReferenceId,
        string jobTitle,
        decimal amount,
        string customerEmail,
        string successUrl,
        string cancelUrl)
    {
        var currency =
            _configuration["Stripe:Currency"] ?? "usd";

        var amountInSmallestUnit =
            (long)Math.Round(amount * 100);

        var options = new SessionCreateOptions
        {
            Mode = "payment",
            CustomerEmail = customerEmail,
            ClientReferenceId = applicationReferenceId,

            SuccessUrl =
                $"{successUrl}?session_id={{CHECKOUT_SESSION_ID}}",


            CancelUrl = cancelUrl,

            LineItems = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions
                {
                    PriceData =
                        new SessionLineItemPriceDataOptions
                        {
                            Currency = currency,
                            UnitAmount = amountInSmallestUnit,

                            ProductData =
                                new SessionLineItemPriceDataProductDataOptions
                                {
                                    Name =
                                        $"Job Application - {jobTitle}"
                                }
                        },

                    Quantity = 1
                }
            },

            Metadata = new Dictionary<string, string>
            {
                ["application_reference_id"] =
                    applicationReferenceId
            }
        };

        var service = new SessionService();

        return await service.CreateAsync(options);
    }
}