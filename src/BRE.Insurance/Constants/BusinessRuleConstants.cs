namespace BRE.Insurance.Constants;

/// <summary>
/// ============================================================
/// BUSINESS RULES CONSTANTS — Central BRE Reference (.NET)
/// ============================================================
/// All numeric thresholds and policy constants used across
/// the insurance platform are defined here for extraction,
/// audit, and AWS Transform analysis.
///
/// Rule IDs follow pattern: BR-MODULE-SEQ
/// ============================================================
/// </summary>
public static class BusinessRuleConstants
{
    // ─────────────────────────────────────────────────
    // MODULE 1: KYC & USER REGISTRATION
    // ─────────────────────────────────────────────────

    /// <summary>BR-KYC-001: Minimum age to purchase any insurance policy</summary>
    public const int MinPolicyholderAge = 18;

    /// <summary>BR-KYC-002: Maximum age for new life insurance enrollment</summary>
    public const int MaxLifeInsuranceEntryAge = 65;

    /// <summary>BR-KYC-003: KYC document must be valid for at least this many months ahead</summary>
    public const int KycDocumentMinValidityMonths = 6;

    /// <summary>BR-KYC-004: OTP expiry for KYC verification (minutes)</summary>
    public const int KycOtpExpiryMinutes = 10;

    /// <summary>BR-KYC-005: Max KYC verification attempts before account lock</summary>
    public const int KycMaxVerificationAttempts = 3;

    // ─────────────────────────────────────────────────
    // MODULE 2: ASSET & PROPERTY
    // ─────────────────────────────────────────────────

    /// <summary>BR-ASSET-001: Minimum insurable property value (INR)</summary>
    public const long MinPropertyValue = 100_000L;

    /// <summary>BR-ASSET-002: Maximum insurable property value (INR)</summary>
    public const long MaxPropertyValue = 50_000_000L;

    /// <summary>BR-ASSET-003: Maximum vehicle age (years) eligible for insurance</summary>
    public const int MaxVehicleAgeYears = 15;

    /// <summary>BR-ASSET-004: VIN must be exactly 17 characters</summary>
    public const int VinLength = 17;

    /// <summary>BR-ASSET-005: Annual vehicle depreciation rate (%)</summary>
    public const double VehicleAnnualDepreciationRate = 0.05;

    // ─────────────────────────────────────────────────
    // MODULE 3: LIFE & HEALTH INSURANCE
    // ─────────────────────────────────────────────────

    /// <summary>BR-LIFE-001: Sum Assured max = this multiple of annual income</summary>
    public const int SumAssuredMaxIncomeMultiple = 20;

    /// <summary>BR-LIFE-002: Waiting period (years) for pre-existing condition coverage</summary>
    public const int PreExistingConditionWaitingYears = 2;

    /// <summary>BR-LIFE-003: Age above which mandatory medical exam is required</summary>
    public const int MedicalExamRequiredAge = 60;

    /// <summary>BR-LIFE-004: Maximum number of dependents per policy</summary>
    public const int MaxDependentsPerPolicy = 6;

    /// <summary>BR-LIFE-005: Minimum Sum Assured for life insurance (INR)</summary>
    public const long MinSumAssured = 100_000L;

    /// <summary>BR-LIFE-006: Smoker premium loading (%)</summary>
    public const double SmokerPremiumLoadingPercent = 25.0;

    // ─────────────────────────────────────────────────
    // MODULE 4: INSURANCE CATEGORY
    // ─────────────────────────────────────────────────

    /// <summary>BR-CAT-001: Minimum employees required for a corporate group policy</summary>
    public const int CorporateMinEmployees = 10;

    /// <summary>BR-CAT-002: Maximum members under individual family floater plan</summary>
    public const int FamilyFloaterMaxMembers = 6;

    /// <summary>BR-CAT-003: Corporate renewal notice days before expiry</summary>
    public const int CorporateRenewalNoticeDays = 45;

    // ─────────────────────────────────────────────────
    // MODULE 5: PAYMENT GATEWAY
    // ─────────────────────────────────────────────────

    /// <summary>BR-PAY-001: Minimum premium payment amount (INR)</summary>
    public const double MinPremiumAmount = 500.0;

    /// <summary>BR-PAY-002: Maximum single transaction limit (INR)</summary>
    public const double MaxSingleTransactionAmount = 1_000_000.0;

    /// <summary>BR-PAY-003: Maximum payment retry attempts</summary>
    public const int MaxPaymentRetryAttempts = 3;

    /// <summary>BR-PAY-004: Payment session timeout (minutes)</summary>
    public const int PaymentSessionTimeoutMinutes = 15;

    /// <summary>BR-PAY-005: Convenience fee for credit card payments (%)</summary>
    public const double CreditCardConvenienceFeePercent = 1.8;

    // ─────────────────────────────────────────────────
    // MODULE 6: SIP
    // ─────────────────────────────────────────────────

    /// <summary>BR-SIP-001: Minimum monthly SIP installment (INR)</summary>
    public const double SipMinMonthlyAmount = 1_000.0;

    /// <summary>BR-SIP-002: Minimum SIP tenure in months</summary>
    public const int SipMinTenureMonths = 12;

    /// <summary>BR-SIP-003: Maximum SIP tenure in months</summary>
    public const int SipMaxTenureMonths = 360;

    /// <summary>BR-SIP-004: Grace period (days) after missed SIP auto-debit</summary>
    public const int SipGracePeriodDays = 30;

    /// <summary>BR-SIP-005: Auto-debit deduction day of month</summary>
    public const int SipAutoDebitDay = 5;

    // ─────────────────────────────────────────────────
    // MODULE 7: NOTIFICATIONS
    // ─────────────────────────────────────────────────

    /// <summary>BR-NOTIF-001: Days before expiry for first renewal reminder</summary>
    public const int RenewalReminderDaysFirst = 30;

    /// <summary>BR-NOTIF-002: Days before expiry for second renewal reminder</summary>
    public const int RenewalReminderDaysSecond = 7;

    /// <summary>BR-NOTIF-003: Max hours for claim status notification</summary>
    public const int ClaimStatusNotificationMaxHours = 24;

    /// <summary>BR-NOTIF-004: Max SMS per policyholder per day</summary>
    public const int MaxSmsPerDay = 5;

    // ─────────────────────────────────────────────────
    // MODULE 8: CANCELLATION & REFUND
    // ─────────────────────────────────────────────────

    /// <summary>BR-CANCEL-001: Cooling-off / free-look period (days)</summary>
    public const int CoolingOffPeriodDays = 15;

    /// <summary>BR-CANCEL-002: Stamp duty deducted on cancellation refund (INR)</summary>
    public const double CancellationStampDuty = 50.0;

    /// <summary>BR-CANCEL-003: Admin charges after cooling-off period (INR)</summary>
    public const double CancellationAdminCharges = 250.0;

    // ─────────────────────────────────────────────────
    // MODULE 9: CLAIM FILING
    // ─────────────────────────────────────────────────

    /// <summary>BR-CLAIM-001: Claim must be filed within N days of incident</summary>
    public const int ClaimFilingWindowDays = 30;

    /// <summary>BR-CLAIM-002: Minimum policy active period (days) before claim eligibility</summary>
    public const int PolicyWaitingPeriodDays = 90;

    /// <summary>BR-CLAIM-004: Maximum supporting documents per claim</summary>
    public const int ClaimMaxDocuments = 10;

    /// <summary>BR-CLAIM-005: Minimum claim amount (INR)</summary>
    public const double ClaimMinAmount = 1_000.0;

    // ─────────────────────────────────────────────────
    // MODULE 10: CLAIM TRACKING
    // ─────────────────────────────────────────────────

    /// <summary>BR-TRACK-001: Hours for surveyor assignment SLA</summary>
    public const int SurveyorAssignmentSlaHours = 48;

    /// <summary>BR-TRACK-002: Working days for claim decision SLA</summary>
    public const int ClaimDecisionSlaWorkingDays = 30;

    /// <summary>BR-TRACK-003: Days for disbursement after approval</summary>
    public const int DisbursementSlaDays = 7;

    // ─────────────────────────────────────────────────
    // MODULE 11: HELPDESK
    // ─────────────────────────────────────────────────

    /// <summary>BR-HELP-001: Hours before ticket auto-escalation</summary>
    public const int HelpdeskEscalationThresholdHours = 72;

    /// <summary>BR-HELP-002: Business hours for callback SLA</summary>
    public const int CallbackSlaBusinessHours = 4;

    /// <summary>BR-HELP-003: Max open tickets per user</summary>
    public const int MaxOpenTicketsPerUser = 5;

    // ─────────────────────────────────────────────────
    // MODULE 12: FEEDBACK
    // ─────────────────────────────────────────────────

    /// <summary>BR-FEEDBACK-001: Feedback window days after claim settlement</summary>
    public const int FeedbackWindowDays = 30;

    /// <summary>BR-FEEDBACK-002: Minimum rating score</summary>
    public const int FeedbackMinRating = 1;

    /// <summary>BR-FEEDBACK-003: Maximum rating score</summary>
    public const int FeedbackMaxRating = 5;

    /// <summary>BR-FEEDBACK-004: Mandatory comment threshold (rating at or below)</summary>
    public const int FeedbackCommentMandatoryBelowRating = 2;

    /// <summary>BR-FEEDBACK-005: Minimum comment length when mandatory</summary>
    public const int FeedbackMinCommentLength = 20;
}
