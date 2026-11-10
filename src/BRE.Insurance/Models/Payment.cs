namespace BRE.Insurance.Models;

public class Payment
{
    public long Id { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public long PolicyId { get; set; }
    public Policy? Policy { get; set; }
    public double Amount { get; set; }
    public double ConvenienceFee { get; set; }
    public double TotalAmount { get; set; }
    public int RetryCount { get; set; }
    public PaymentChannel Channel { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Initiated;
    public DateTime InitiatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public DateTime SessionExpiresAt { get; set; }
}

public enum PaymentChannel { CreditCard, DebitCard, NetBanking, Upi, Wallet }
public enum PaymentStatus  { Initiated, Pending, Success, Failed, Refunded, Expired }
