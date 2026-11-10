namespace BRE.Insurance.Models;

public class User
{
    public long Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
    public Gender Gender { get; set; }
    public string Email { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Pincode { get; set; } = string.Empty;
    public KycStatus KycStatus { get; set; } = KycStatus.Pending;
    public bool KycVerified { get; set; }
    public int KycVerificationAttempts { get; set; }
    public DateTime? KycVerifiedAt { get; set; }
    public List<KycDocument> KycDocuments { get; set; } = new();
    public List<Policy> Policies { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public enum Gender   { Male, Female, Other }
public enum KycStatus { Pending, UnderReview, Verified, Rejected }
