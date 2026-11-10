namespace BRE.Insurance.Models;

public class Policy
{
    public long Id { get; set; }
    public string PolicyNumber { get; set; } = string.Empty;
    public long UserId { get; set; }
    public User? User { get; set; }
    public PolicyType PolicyType { get; set; }
    public PolicyCategory PolicyCategory { get; set; }
    public PolicyStatus Status { get; set; } = PolicyStatus.Draft;
    public double PremiumAmount { get; set; }
    public double SumAssured { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public DateOnly NextDueDate { get; set; }
    public PaymentFrequency PaymentFrequency { get; set; }
    public bool SipEnabled { get; set; }
    public int DependentsCount { get; set; }
    public bool HasPreExistingCondition { get; set; }
    public bool IsSmoker { get; set; }
    public double AnnualIncome { get; set; }
    public List<Claim> Claims { get; set; } = new();
    public List<Payment> Payments { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public enum PolicyType      { Life, Health, Vehicle, Property, GroupHealth, CommercialProperty, Liability }
public enum PolicyCategory  { Individual, FamilyFloater, Corporate }
public enum PolicyStatus    { Draft, Active, Lapsed, Cancelled, Expired, UnderReview }
public enum PaymentFrequency { Monthly, Quarterly, Biannually, Annually }
