namespace BRE.Insurance.Models;

public class SupportTicket
{
    public long Id { get; set; }
    public string TicketNumber { get; set; } = string.Empty;
    public long UserId { get; set; }
    public User? User { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool CallbackRequested { get; set; }
    public TicketStatus Status { get; set; } = TicketStatus.Open;
    public TicketPriority Priority { get; set; } = TicketPriority.Medium;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? EscalatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public DateTime? CallbackScheduledAt { get; set; }
}

public enum TicketStatus   { Open, InProgress, Escalated, Resolved, Closed }
public enum TicketPriority { Low, Medium, High, Critical }
