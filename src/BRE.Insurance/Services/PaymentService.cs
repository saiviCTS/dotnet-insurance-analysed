using BRE.Insurance.Constants;
using BRE.Insurance.Models;

namespace BRE.Insurance.Services;

/// <summary>
/// Payment Service — Module 5: Payment Gateway Integration
///
/// Business Rules:
///   BR-PAY-001 : Min premium = INR 500
///   BR-PAY-002 : Max single transaction = INR 10,00,000
///   BR-PAY-003 : Max payment retries = 3
///   BR-PAY-004 : Payment session expires in 15 minutes
///   BR-PAY-005 : Credit card convenience fee = 1.8%
/// </summary>
public class PaymentService
{
    public void ValidatePaymentAmount(double amount)
    {
        if (amount < BusinessRuleConstants.MinPremiumAmount)
            throw new ArgumentException(
                $"BR-PAY-001 VIOLATED: Amount {amount:C} below minimum " +
                $"{BusinessRuleConstants.MinPremiumAmount:C}");

        if (amount > BusinessRuleConstants.MaxSingleTransactionAmount)
            throw new ArgumentException(
                $"BR-PAY-002 VIOLATED: Amount {amount:C} exceeds max single transaction " +
                $"{BusinessRuleConstants.MaxSingleTransactionAmount:C}");
    }

    public void ValidateRetryEligibility(Payment payment)
    {
        if (payment.RetryCount >= BusinessRuleConstants.MaxPaymentRetryAttempts)
            throw new InvalidOperationException(
                $"BR-PAY-003 VIOLATED: Max retry attempts " +
                $"({BusinessRuleConstants.MaxPaymentRetryAttempts}) reached.");
    }

    public bool IsPaymentSessionValid(Payment payment)
    {
        if (DateTime.UtcNow > payment.SessionExpiresAt)
        {
            payment.Status = PaymentStatus.Expired;
            return false;
        }
        return true;
    }

    public double CalculateConvenienceFee(double amount, PaymentChannel channel)
    {
        if (channel == PaymentChannel.CreditCard)
            return amount * (BusinessRuleConstants.CreditCardConvenienceFeePercent / 100.0);
        return 0.0;
    }

    public Payment Initiate(double baseAmount, PaymentChannel channel)
    {
        ValidatePaymentAmount(baseAmount);
        double fee   = CalculateConvenienceFee(baseAmount, channel);
        double total = baseAmount + fee;

        return new Payment
        {
            Amount           = baseAmount,
            ConvenienceFee   = fee,
            TotalAmount      = total,
            Channel          = channel,
            Status           = PaymentStatus.Initiated,
            RetryCount       = 0,
            InitiatedAt      = DateTime.UtcNow,
            SessionExpiresAt = DateTime.UtcNow.AddMinutes(
                BusinessRuleConstants.PaymentSessionTimeoutMinutes)
        };
    }
}
