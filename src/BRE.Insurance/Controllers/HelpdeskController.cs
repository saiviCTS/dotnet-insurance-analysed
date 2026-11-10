using BRE.Insurance.Constants;
using BRE.Insurance.Data;
using BRE.Insurance.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BRE.Insurance.Controllers;

[ApiController]
[Route("api/helpdesk")]
public class HelpdeskController : ControllerBase
{
    private readonly InsuranceDbContext _db;

    public HelpdeskController(InsuranceDbContext db) => _db = db;

    // GET api/helpdesk/tickets
    [HttpGet("tickets")]
    public async Task<ActionResult<IEnumerable<SupportTicket>>> GetAll()
        => Ok(await _db.SupportTickets.Include(t => t.User).ToListAsync());

    // GET api/helpdesk/tickets/5
    [HttpGet("tickets/{id:long}")]
    public async Task<ActionResult<SupportTicket>> GetById(long id)
    {
        var ticket = await _db.SupportTickets.Include(t => t.User).FirstOrDefaultAsync(t => t.Id == id);
        return ticket is null ? NotFound() : Ok(ticket);
    }

    // GET api/helpdesk/users/5/tickets
    [HttpGet("users/{userId:long}/tickets")]
    public async Task<ActionResult<IEnumerable<SupportTicket>>> GetByUser(long userId)
        => Ok(await _db.SupportTickets.Where(t => t.UserId == userId).ToListAsync());

    // POST api/helpdesk/users/5/tickets
    [HttpPost("users/{userId:long}/tickets")]
    public async Task<ActionResult<SupportTicket>> Create(
        long userId, [FromBody] CreateTicketRequest req)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user is null) return NotFound();

        // BR-HELP-003: Max 5 open tickets per user
        var openStatuses = new[] { TicketStatus.Open, TicketStatus.InProgress, TicketStatus.Escalated };
        int openCount    = await _db.SupportTickets
            .CountAsync(t => t.UserId == userId && openStatuses.Contains(t.Status));

        if (openCount >= BusinessRuleConstants.MaxOpenTicketsPerUser)
            return BadRequest($"BR-HELP-003 VIOLATED: Max {BusinessRuleConstants.MaxOpenTicketsPerUser} open tickets per user.");

        var ticket = new SupportTicket
        {
            UserId            = userId,
            User              = user,
            TicketNumber      = $"TKT-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
            Subject           = req.Subject,
            Description       = req.Description,
            CallbackRequested = req.CallbackRequested,
            Status            = TicketStatus.Open,
            Priority          = TicketPriority.Medium,
            CreatedAt         = DateTime.UtcNow
        };

        _db.SupportTickets.Add(ticket);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = ticket.Id }, ticket);
    }

    // POST api/helpdesk/tickets/5/escalate-check
    [HttpPost("tickets/{id:long}/escalate-check")]
    public async Task<ActionResult<object>> EscalateCheck(long id)
    {
        var ticket = await _db.SupportTickets.FindAsync(id);
        if (ticket is null) return NotFound();

        // BR-HELP-001: Auto-escalate if open > 72 hours
        if (ticket.Status is TicketStatus.Open or TicketStatus.InProgress)
        {
            double hoursOpen = (DateTime.UtcNow - ticket.CreatedAt).TotalHours;
            if (hoursOpen >= BusinessRuleConstants.HelpdeskEscalationThresholdHours
                && ticket.Status != TicketStatus.Escalated)
            {
                ticket.Status      = TicketStatus.Escalated;
                ticket.Priority    = TicketPriority.High;
                ticket.EscalatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }
        }

        return Ok(new { ticketId = id, ticket.Status, escalated = ticket.Status == TicketStatus.Escalated });
    }

    // PATCH api/helpdesk/tickets/5/resolve
    [HttpPatch("tickets/{id:long}/resolve")]
    public async Task<ActionResult<SupportTicket>> Resolve(long id)
    {
        var ticket = await _db.SupportTickets.FindAsync(id);
        if (ticket is null) return NotFound();

        ticket.Status     = TicketStatus.Resolved;
        ticket.ResolvedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Ok(ticket);
    }
}

public record CreateTicketRequest(string Subject, string Description, bool CallbackRequested);
