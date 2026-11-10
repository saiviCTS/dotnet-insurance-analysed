using BRE.Insurance.Constants;
using BRE.Insurance.Models;

namespace BRE.Insurance.Services;

/// <summary>
/// Claim Service — Modules 9 &amp; 10: Claim Filing &amp; Tracking
///
/// Business Rules:
///   BR-CLAIM-001 : Claim filed within 30 days of incident
///   BR-CLAIM-002 : Policy active >= 90 days before claim
///   BR-CLAIM-003 : Claim amount &lt;= Sum Assured
///   BR-CLAIM-004 : Max 10 documents per claim
///   BR-CLAIM-005 : Min claim amount = INR 1,000
///   BR-TRACK-001 : Surveyor assigned within 48 hours
///   BR-TRACK-002 : Decision within 30 working days
///   BR-TRACK-003 : Disbursement within 7 days of approval
/// </summary>
public class ClaimService
{
    public void ValidateClaimFilingWindow(DateOnly incidentDate)
    {
        int daysSince = DateOnly.FromDateTime(DateTime.Today).DayNumber - incidentDate.DayNumber;
        if (daysSince > BusinessRuleConstants.ClaimFilingWindowDays)
            throw new ArgumentException(
                $"BR-CLAIM-001 VIOLATED: Claim must be filed within " +
                $"{BusinessRuleConstants.ClaimFilingWindowDays} days. Days elapsed: {daysSince}");
    }

    public void ValidatePolicyWaitingPeriod(Policy policy)
    {
        int daysActive = DateOnly.FromDateTime(DateTime.Today).DayNumber - policy.StartDate.DayNumber;
        if (daysActive < BusinessRuleConstants.PolicyWaitingPeriodDays)
            throw new InvalidOperationException(
                $"BR-CLAIM-002 VIOLATED: Policy must be active for " +
                $"{BusinessRuleConstants.PolicyWaitingPeriodDays} days. Active: {daysActive}");
    }

    public void ValidateClaimAmount(double claimAmount, Policy policy)
    {
        if (claimAmount > policy.SumAssured)
            throw new ArgumentException(
                $"BR-CLAIM-003 VIOLATED: Claim {claimAmount:C} exceeds Sum Assured {policy.SumAssured:C}");
        if (claimAmount < BusinessRuleConstants.ClaimMinAmount)
            throw new ArgumentException(
                $"BR-CLAIM-005 VIOLATED: Claim {claimAmount:C} below minimum {BusinessRuleConstants.ClaimMinAmount:C}");
    }

    public void ValidateDocumentCount(int count)
    {
        if (count > BusinessRuleConstants.ClaimMaxDocuments)
            throw new ArgumentException(
                $"BR-CLAIM-004 VIOLATED: Cannot attach more than {BusinessRuleConstants.ClaimMaxDocuments} documents.");
    }

    public Claim FileNewClaim(Policy policy, DateOnly incidentDate,
                              string description, double claimAmount)
    {
        ValidateClaimFilingWindow(incidentDate);
        ValidatePolicyWaitingPeriod(policy);
        ValidateClaimAmount(claimAmount, policy);

        if (policy.Status != PolicyStatus.Active)
            throw new InvalidOperationException($"Policy not ACTIVE. Status: {policy.Status}");

        return new Claim
        {
            Policy             = policy,
            PolicyId           = policy.Id,
            IncidentDate       = incidentDate,
            FiledDate          = DateOnly.FromDateTime(DateTime.Today),
            IncidentDescription = description,
            ClaimAmount        = claimAmount,
            Status             = ClaimStatus.Submitted,
            CreatedAt          = DateTime.UtcNow
        };
    }

    // ── BR-TRACK-001 ──────────────────────────────────────────────────────────
    public bool IsSurveyorAssignmentOverdue(Claim claim)
    {
        if (claim.Status == ClaimStatus.Submitted && claim.SurveyorName is null)
        {
            double hoursElapsed = (DateTime.UtcNow - claim.CreatedAt).TotalHours;
            return hoursElapsed > BusinessRuleConstants.SurveyorAssignmentSlaHours;
        }
        return false;
    }

    // ── BR-TRACK-003 ──────────────────────────────────────────────────────────
    public bool IsDisbursementOverdue(Claim claim)
    {
        if (claim.Status == ClaimStatus.Approved && claim.ApprovedAt.HasValue)
        {
            double daysAfterApproval = (DateTime.UtcNow - claim.ApprovedAt.Value).TotalDays;
            return daysAfterApproval > BusinessRuleConstants.DisbursementSlaDays;
        }
        return false;
    }
}
