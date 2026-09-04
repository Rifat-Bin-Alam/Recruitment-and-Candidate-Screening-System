using System.ComponentModel.DataAnnotations;
using RecruitmentAndCandidateScreeningSystem.Models.Enums;

namespace RecruitmentAndCandidateScreeningSystem.Models;

public class Payment
{
    public int Id { get; set; }

    public int JobApplicationId { get; set; }

    public decimal Amount { get; set; }

    public PaymentMethod PaymentMethod { get; set; }

    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    [StringLength(200)]
    public string? StripePaymentIntentId { get; set; }

    [StringLength(200)]
    public string? StripeSessionId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? PaidAt { get; set; }

    public string? TransactionId { get; set; }
    public JobApplication JobApplication { get; set; } = null!;
}