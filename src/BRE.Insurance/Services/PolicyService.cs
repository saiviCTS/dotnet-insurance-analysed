using BRE.Insurance.Constants;
using BRE.Insurance.Models;

namespace BRE.Insurance.Services;

/// <summary>
/// Policy Service — Modules 3 &amp; 4: Life/Health + Insurance Category
///
/// Business Rules:
///   BR-LIFE-001 : Sum Assured &lt;= 20× annual income
///   BR-LIFE-002 : Pre-existing condition = 2-year waiting period
///   BR-LIFE-003 : Age &gt; 60 requires mandatory medical exam
///   BR-LIFE-004 : Max 6 dependents per policy
///   BR-LIFE-005 : Min Sum Assured = INR 1,00,000
///   BR-LIFE-006 : Smoker loading = +25% premium
///   BR-CAT-001  : Corporate policy needs min 10 employees
///   BR-CAT-002  : Family floater max 6 members
///   BR-CAT-003  : Corporate renewal notice 45 days before expiry
/// </summary>
public class PolicyService
{
    // ── BR-LIFE-001 & BR-LIFE-005: Sum Assured Bounds ────────────────────────
    public void ValidateSumAssured(double sumAssured, double annualIncome)
    {
        double maxAllowed = annualIncome * BusinessRuleConstants.SumAssuredMaxIncomeMultiple;
        if (sumAssured > maxAllowed)
            throw new ArgumentException(
                $"BR-LIFE-001 VIOLATED: Sum Assured {sumAssured:C} exceeds maximum " +
                $"{maxAllowed:C} (income × {BusinessRuleConstants.SumAssuredMaxIncomeMultiple})");

        if (sumAssured < BusinessRuleConstants.MinSumAssured)
            throw new ArgumentException(
                $"BR-LIFE-005 VIOLATED: Sum Assured {sumAssured:C} below minimum " +
                $"{BusinessRuleConstants.MinSumAssured:C}");
    }

    // ── BR-LIFE-003: Medical Exam Gate ────────────────────────────────────────
    public bool IsMedicalExamRequired(DateOnly dateOfBirth)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        int age   = today.Year - dateOfBirth.Year;
        if (dateOfBirth > today.AddYears(-age)) age--;
        return age > BusinessRuleConstants.MedicalExamRequiredAge;
    }

    // ── BR-LIFE-004: Dependents Limit ─────────────────────────────────────────
    public void ValidateDependentsCount(int count)
    {
        if (count > BusinessRuleConstants.MaxDependentsPerPolicy)
            throw new ArgumentException(
                $"BR-LIFE-004 VIOLATED: Dependents {count} exceeds max " +
                $"{BusinessRuleConstants.MaxDependentsPerPolicy}");
    }

    // ── BR-LIFE-006: Smoker Premium Loading ───────────────────────────────────
    public double ApplySmokingLoading(double basePremium, bool isSmoker)
    {
        if (!isSmoker) return basePremium;
        double loading = basePremium * (BusinessRuleConstants.SmokerPremiumLoadingPercent / 100.0);
        return basePremium + loading;
    }

    // ── BR-LIFE-002: Pre-existing Waiting Period ──────────────────────────────
    public DateOnly CalculateWaitingPeriodEnd(Policy policy)
    {
        if (policy.HasPreExistingCondition)
            return policy.StartDate.AddYears(BusinessRuleConstants.PreExistingConditionWaitingYears);
        return policy.StartDate;
    }

    // ── BR-CAT-001: Corporate Minimum Employees ───────────────────────────────
    public void ValidateCorporatePolicySize(int employeeCount)
    {
        if (employeeCount < BusinessRuleConstants.CorporateMinEmployees)
            throw new ArgumentException(
                $"BR-CAT-001 VIOLATED: Corporate policy requires min " +
                $"{BusinessRuleConstants.CorporateMinEmployees} employees. Got: {employeeCount}");
    }

    // ── BR-CAT-002: Family Floater Members ────────────────────────────────────
    public void ValidateFamilyFloaterMembers(int memberCount)
    {
        if (memberCount > BusinessRuleConstants.FamilyFloaterMaxMembers)
            throw new ArgumentException(
                $"BR-CAT-002 VIOLATED: Family floater allows max " +
                $"{BusinessRuleConstants.FamilyFloaterMaxMembers} members. Got: {memberCount}");
    }

    // ── BR-CAT-003: Corporate Renewal Window ──────────────────────────────────
    public bool IsCorporateRenewalWindowOpen(Policy policy)
    {
        DateOnly windowStart = policy.EndDate.AddDays(-BusinessRuleConstants.CorporateRenewalNoticeDays);
        return DateOnly.FromDateTime(DateTime.Today) >= windowStart;
    }
}
