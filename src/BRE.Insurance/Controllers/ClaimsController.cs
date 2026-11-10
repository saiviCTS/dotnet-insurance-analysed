using BRE.Insurance.Data;
using BRE.Insurance.Models;
using BRE.Insurance.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BRE.Insurance.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClaimsController : ControllerBase
{
    private readonly InsuranceDbContext _db;
    private readonly ClaimService _claimService;

    public ClaimsController(InsuranceDbContext db, ClaimService claimService)
    {
        _db          = db;
        _claimService = claimService;
    }

    // GET api/claims
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Claim>>> GetAll()
        => Ok(await _db.Claims.Include(c => c.Policy).ToListAsync());

    // GET api/claims/5
    [HttpGet("{id:long}")]
    public async Task<ActionResult<Claim>> GetById(long id)
    {
        var claim = await _db.Claims
            .Include(c => c.Policy)
            .Include(c => c.Documents)
            .FirstOrDefaultAsync(c => c.Id == id);
        return claim is null ? NotFound() : Ok(claim);
    }

    // GET api/claims/policies/5
    [HttpGet("policies/{policyId:long}")]
    public async Task<ActionResult<IEnumerable<Claim>>> GetByPolicy(long policyId)
        => Ok(await _db.Claims.Where(c => c.PolicyId == policyId).ToListAsync());

    // POST api/claims/policies/5
    [HttpPost("policies/{policyId:long}")]
    public async Task<ActionResult<Claim>> File(long policyId, [FromBody] FileClaimRequest req)
    {
        var policy = await _db.Policies
            .Include(p => p.Claims)
            .FirstOrDefaultAsync(p => p.Id == policyId);
        if (policy is null) return NotFound();

        var claim = _claimService.FileNewClaim(policy, req.IncidentDate, req.Description, req.ClaimAmount);
        claim.ClaimNumber = $"CLM-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";

        _db.Claims.Add(claim);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = claim.Id }, claim);
    }

    // GET api/claims/5/tracking
    [HttpGet("{id:long}/tracking")]
    public async Task<ActionResult<object>> Tracking(long id)
    {
        var claim = await _db.Claims.FindAsync(id);
        if (claim is null) return NotFound();

        return Ok(new
        {
            claimId                    = id,
            claim.Status,
            surveyorAssignmentOverdue  = _claimService.IsSurveyorAssignmentOverdue(claim),
            disbursementOverdue        = _claimService.IsDisbursementOverdue(claim)
        });
    }

    // PATCH api/claims/5/status?newStatus=UnderReview&actor=John
    [HttpPatch("{id:long}/status")]
    public async Task<ActionResult<Claim>> TransitionStatus(
        long id, [FromQuery] ClaimStatus newStatus, [FromQuery] string? actor)
    {
        var claim = await _db.Claims.FindAsync(id);
        if (claim is null) return NotFound();

        // Enforce valid transitions
        var valid = (claim.Status, newStatus) switch
        {
            (ClaimStatus.Submitted,       ClaimStatus.UnderReview)      => true,
            (ClaimStatus.UnderReview,     ClaimStatus.SurveyorAssigned) => true,
            (ClaimStatus.UnderReview,     ClaimStatus.Approved)         => true,
            (ClaimStatus.UnderReview,     ClaimStatus.Rejected)         => true,
            (ClaimStatus.SurveyorAssigned, ClaimStatus.Approved)        => true,
            (ClaimStatus.SurveyorAssigned, ClaimStatus.Rejected)        => true,
            (ClaimStatus.Approved,        ClaimStatus.Disbursed)        => true,
            _ => false
        };

        if (!valid)
            return BadRequest($"Cannot transition from {claim.Status} to {newStatus}.");

        claim.Status = newStatus;
        if (newStatus == ClaimStatus.SurveyorAssigned) { claim.SurveyorAssignedAt = DateTime.UtcNow; claim.SurveyorName = actor; }
        if (newStatus == ClaimStatus.Approved)          claim.ApprovedAt  = DateTime.UtcNow;
        if (newStatus == ClaimStatus.Disbursed)         claim.DisbursedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Ok(claim);
    }

    // DELETE api/claims/5
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        var claim = await _db.Claims.FindAsync(id);
        if (claim is null) return NotFound();

        _db.Claims.Remove(claim);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}

public record FileClaimRequest(DateOnly IncidentDate, string Description, double ClaimAmount);
