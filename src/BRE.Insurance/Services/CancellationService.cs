using BRE.Insurance.Constants;
using BRE.Insurance.Models;

namespace BRE.Insurance.Services;

/// <summary>
/// Cancellation Service — Module 8: Cancellation &amp; Refund
///
/// Business Rules:
///   BR-CANCEL-001 : Cooling-off = 15 days from policy start
///   BR-CANCEL-002 : Stamp duty INR 50 deducted on refund
///   BR-CANCEL-003 : Admin charges INR 250 after cooling-off period
///   BR-CANCEL-004 : No refund if active/approved claim exists
/// </summary>
public class CancellationService
{
    // ── BR-CANCEL-004 ────────────────────────────────────────────────────────
    public void ValidateCancellationEligibility(Policy policy)
    {
        bool hasActiveClaim = policy.Claims.Any(c =>
            c.Status is ClaimStatus.Submitted or ClaimStatus.UnderReview
                     or ClaimStatus.SurveyorAssigned or ClaimStatus.Approved);

        if (hasActiveClaim)
            throw new InvalidOperationException(
                "BR-CANCEL-004 VIOLATED: Cannot cancel policy with an active or approved claim.");
    }

    // ── BR-CANCEL-001 ────────────────────────────────────────────────────────
    public bool IsWithinCoolingOffPeriod(Policy policy)
    {
        int daysSinceStart = DateOnly.FromDateTime(DateTime.Today).DayNumber - policy.StartDate.DayNumber;
        return daysSinceStart <= BusinessRuleConstants.CoolingOffPeriodDays;
    }

    // ── Pro-rata Refund Calculation ───────────────────────────────────────────
    public double CalculateRefundAmount(Policy policy)
    {
        ValidateCancellationEligibility(policy);

        int totalDays     = policy.EndDate.DayNumber - policy.StartDate.DayNumber;
        int remainingDays = policy.EndDate.DayNumber - DateOnly.FromDateTime(DateTime.Today).DayNumber;

        if (remainingDays <= 0) return 0.0;

        double proRata = ((double)remainingDays / totalDays) * policy.PremiumAmount;

        // BR-CANCEL-002: Always deduct stamp duty
        proRata -= BusinessRuleConstants.CancellationStampDuty;

        // BR-CANCEL-003: Admin charges outside cooling-off period
        if (!IsWithinCoolingOffPeriod(policy))
            proRata -= BusinessRuleConstants.CancellationAdminCharges;

        return Math.Max(0.0, Math.Round(proRata, 2));
    }
}
