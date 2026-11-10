using BRE.Insurance.Data;
using BRE.Insurance.Models;
using BRE.Insurance.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BRE.Insurance.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FeedbackController : ControllerBase
{
    private readonly InsuranceDbContext _db;
    private readonly FeedbackService _feedbackService;

    public FeedbackController(InsuranceDbContext db, FeedbackService feedbackService)
    {
        _db             = db;
        _feedbackService = feedbackService;
    }

    // GET api/feedback
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Feedback>>> GetAll()
        => Ok(await _db.Feedbacks.Include(f => f.User).ToListAsync());

    // GET api/feedback/5
    [HttpGet("{id:long}")]
    public async Task<ActionResult<Feedback>> GetById(long id)
    {
        var feedback = await _db.Feedbacks.FindAsync(id);
        return feedback is null ? NotFound() : Ok(feedback);
    }

    // GET api/feedback/users/5
    [HttpGet("users/{userId:long}")]
    public async Task<ActionResult<IEnumerable<Feedback>>> GetByUser(long userId)
        => Ok(await _db.Feedbacks.Where(f => f.UserId == userId).ToListAsync());

    // GET api/feedback/claims/5
    [HttpGet("claims/{claimId:long}")]
    public async Task<ActionResult<IEnumerable<Feedback>>> GetByClaim(long claimId)
        => Ok(await _db.Feedbacks.Where(f => f.ClaimId == claimId).ToListAsync());

    // POST api/feedback/users/5/claims/10
    [HttpPost("users/{userId:long}/claims/{claimId:long}")]
    public async Task<ActionResult<Feedback>> SubmitClaimFeedback(
        long userId, long claimId, [FromBody] SubmitFeedbackRequest req)
    {
        var user  = await _db.Users.FindAsync(userId);
        var claim = await _db.Claims.FindAsync(claimId);
        if (user is null || claim is null) return NotFound();

        var feedback = _feedbackService.Submit(user, claim, req.Rating, req.Comment);

        _db.Feedbacks.Add(feedback);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = feedback.Id }, feedback);
    }
}

public record SubmitFeedbackRequest(int Rating, string? Comment);
