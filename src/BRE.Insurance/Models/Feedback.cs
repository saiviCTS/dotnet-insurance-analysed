namespace BRE.Insurance.Models;

public class Feedback
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public User? User { get; set; }
    public long? ClaimId { get; set; }
    public Claim? Claim { get; set; }
    public long? TicketId { get; set; }
    public SupportTicket? Ticket { get; set; }
    public int Rating { get; set; }       // 1-5
    public string? Comment { get; set; }  // Mandatory if Rating <= 2
    public FeedbackType FeedbackType { get; set; }
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
}

public enum FeedbackType { PostClaim, PostSupport }
