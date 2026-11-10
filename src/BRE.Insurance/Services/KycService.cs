using BRE.Insurance.Constants;
using BRE.Insurance.Models;

namespace BRE.Insurance.Services;

/// <summary>
/// KYC Service — Module 1: User Registration &amp; KYC Verification
///
/// Business Rules:
///   BR-KYC-001 : Minimum policyholder age = 18
///   BR-KYC-002 : KYC must be VERIFIED before any policy issuance
///   BR-KYC-003 : KYC document must have >= 6 months validity remaining
///   BR-KYC-005 : Account locked after 3 failed KYC verification attempts
/// </summary>
public class KycService
{
    // ── BR-KYC-001: Age Eligibility ──────────────────────────────────────────
    public bool IsAgeEligible(DateOnly dateOfBirth)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        int age   = today.Year - dateOfBirth.Year;
        if (dateOfBirth > today.AddYears(-age)) age--;

        if (age < BusinessRuleConstants.MinPolicyholderAge)
            throw new ArgumentException(
                $"BR-KYC-001 VIOLATED: Policyholder must be at least " +
                $"{BusinessRuleConstants.MinPolicyholderAge} years old. Current age: {age}");
        return true;
    }

    // ── BR-KYC-003: Document Validity ────────────────────────────────────────
    public bool IsDocumentValid(KycDocument document)
    {
        if (document.ExpiryDate is null) return true;

        DateOnly minValidUntil = DateOnly.FromDateTime(
            DateTime.Today.AddMonths(BusinessRuleConstants.KycDocumentMinValidityMonths));

        if (document.ExpiryDate < minValidUntil)
            throw new ArgumentException(
                $"BR-KYC-003 VIOLATED: Document {document.DocumentType} must be valid for " +
                $"at least {BusinessRuleConstants.KycDocumentMinValidityMonths} more months. " +
                $"Expiry: {document.ExpiryDate}");
        return true;
    }

    // ── BR-KYC-005: Attempt Lock ──────────────────────────────────────────────
    public void IncrementVerificationAttempt(User user)
    {
        user.KycVerificationAttempts++;
        if (user.KycVerificationAttempts >= BusinessRuleConstants.KycMaxVerificationAttempts)
        {
            user.KycStatus = KycStatus.Rejected;
            throw new InvalidOperationException(
                $"BR-KYC-005 VIOLATED: Account locked after " +
                $"{BusinessRuleConstants.KycMaxVerificationAttempts} failed KYC attempts.");
        }
    }

    // ── BR-KYC-002: Policy Gating ─────────────────────────────────────────────
    public void AssertKycVerified(User user)
    {
        if (!user.KycVerified || user.KycStatus != KycStatus.Verified)
            throw new InvalidOperationException(
                $"BR-KYC-002 VIOLATED: KYC must be VERIFIED before policy issuance. " +
                $"Current status: {user.KycStatus}");
    }
}
