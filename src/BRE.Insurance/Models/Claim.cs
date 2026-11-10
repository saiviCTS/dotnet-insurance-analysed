namespace BRE.Insurance.Models;

public class Claim
{
    public long Id { get; set; }
    public string ClaimNumber { get; set; } = string.Empty;
    public long PolicyId { get; set; }
    public Policy? Policy { get; set; }
    public DateOnly IncidentDate { get; set; }
    public DateOnly FiledDate { get; set; }
    public string IncidentDescription { get; set; } = string.Empty;
    public double ClaimAmount { get; set; }
    public ClaimStatus Status { get; set; } = ClaimStatus.Submitted;
    public string? SurveyorName { get; set; }
    public DateTime? SurveyorAssignedAt { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? DisbursedAt { get; set; }
    public double ApprovedAmount { get; set; }
    public List<ClaimDocument> Documents { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public enum ClaimStatus { Submitted, UnderReview, SurveyorAssigned, Approved, Rejected, Disbursed }
