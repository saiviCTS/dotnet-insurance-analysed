using BRE.Insurance.Constants;
using BRE.Insurance.Models;
using BRE.Insurance.Rules;
using BRE.Insurance.Services;
using Xunit;

namespace BRE.Insurance.Tests;

/// <summary>
/// Business Rules Extraction Test Suite (.NET 8)
/// Validates every embedded business rule is enforced correctly.
/// Each test is labelled with its Rule ID.
/// </summary>
public class BusinessRulesTests
{
    readonly KycService          _kyc          = new();
    readonly PolicyService       _policy       = new();
    readonly PaymentService      _payment      = new();
    readonly ClaimService        _claim        = new();
    readonly CancellationService _cancellation = new();
    readonly FeedbackService     _feedback     = new();

    // ── BR-KYC-001 ────────────────────────────────────────────────────────────
    [Fact(DisplayName = "BR-KYC-001: Underage applicant rejected")]
    public void Kyc001_UnderageApplicantRejected()
    {
        var dob = DateOnly.FromDateTime(DateTime.Today.AddYears(-16));
        Assert.Throws<ArgumentException>(() => _kyc.IsAgeEligible(dob));
    }

    [Fact(DisplayName = "BR-KYC-001: Adult applicant accepted")]
    public void Kyc001_AdultApplicantAccepted()
    {
        var dob = DateOnly.FromDateTime(DateTime.Today.AddYears(-25));
        Assert.True(_kyc.IsAgeEligible(dob));
    }

    // ── BR-LIFE-001 ──────────────────────────────────────────────────────────
    [Fact(DisplayName = "BR-LIFE-001: Sum Assured > 20x income rejected")]
    public void Life001_SumAssuredExceedsIncomeMultiple()
    {
        Assert.Throws<ArgumentException>(() =>
            _policy.ValidateSumAssured(11_000_000, 500_000));
    }

    // ── BR-LIFE-006 ──────────────────────────────────────────────────────────
    [Fact(DisplayName = "BR-LIFE-006: Smoker premium loading = 25%")]
    public void Life006_SmokerPremiumLoading()
    {
        double loaded = _policy.ApplySmokingLoading(10_000, isSmoker: true);
        Assert.Equal(12_500.0, loaded, precision: 2);
    }

    // ── BR-PAY-001 ───────────────────────────────────────────────────────────
    [Fact(DisplayName = "BR-PAY-001: Premium below INR 500 rejected")]
    public void Pay001_LowAmountRejected()
    {
        Assert.Throws<ArgumentException>(() => _payment.ValidatePaymentAmount(499.0));
    }

    // ── BR-PAY-005 ───────────────────────────────────────────────────────────
    [Fact(DisplayName = "BR-PAY-005: Credit card convenience fee = 1.8%")]
    public void Pay005_CreditCardFee()
    {
        double fee = _payment.CalculateConvenienceFee(10_000, PaymentChannel.CreditCard);
        Assert.Equal(180.0, fee, precision: 2);
    }

    [Fact(DisplayName = "BR-PAY-005: UPI has zero fee")]
    public void Pay005_UpiFee()
    {
        double fee = _payment.CalculateConvenienceFee(10_000, PaymentChannel.Upi);
        Assert.Equal(0.0, fee, precision: 2);
    }

    // ── BR-CLAIM-001 ─────────────────────────────────────────────────────────
    [Fact(DisplayName = "BR-CLAIM-001: Claim after 30 days rejected")]
    public void Claim001_LateClaimRejected()
    {
        var oldDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-31));
        Assert.Throws<ArgumentException>(() => _claim.ValidateClaimFilingWindow(oldDate));
    }

    // ── BR-CANCEL-001 ────────────────────────────────────────────────────────
    [Fact(DisplayName = "BR-CANCEL-001: Policy within 15 days is in cooling-off")]
    public void Cancel001_CoolingOffDetected()
    {
        var policy = new Policy
        {
            StartDate     = DateOnly.FromDateTime(DateTime.Today.AddDays(-10)),
            EndDate       = DateOnly.FromDateTime(DateTime.Today.AddYears(1)),
            PremiumAmount = 12_000,
            Claims        = new List<Claim>()
        };
        Assert.True(_cancellation.IsWithinCoolingOffPeriod(policy));
    }

    // ── BR-FEEDBACK-004 ──────────────────────────────────────────────────────
    [Fact(DisplayName = "BR-FEEDBACK-004: Rating <= 2 without comment rejected")]
    public void Feedback004_MandatoryCommentEnforced()
    {
        Assert.Throws<ArgumentException>(() => _feedback.ValidateComment(1, ""));
    }

    [Fact(DisplayName = "BR-FEEDBACK-004: Rating >= 3 without comment accepted")]
    public void Feedback004_NoCommentForHighRating()
    {
        var exception = Record.Exception(() => _feedback.ValidateComment(3, null));
        Assert.Null(exception);
    }

    // ── BRE Extractor ────────────────────────────────────────────────────────
    [Fact(DisplayName = "BRE Catalog: All rules extractable via reflection")]
    public void Bre_AllRulesExtractable()
    {
        var extractor = new BusinessRuleExtractor();
        var rules = extractor.ExtractAllRules();
        Assert.NotEmpty(rules);
        extractor.PrintRulesCatalog();
        Console.WriteLine($"\nTotal rules extracted: {rules.Count}");
    }
}
