using BRE.Insurance.Data;
using BRE.Insurance.Models;
using BRE.Insurance.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BRE.Insurance.Controllers;

[ApiController]
[Route("api/policies/{policyId:long}/cancellation")]
public class CancellationController : ControllerBase
{
    private readonly InsuranceDbContext _db;
    private readonly CancellationService _cancellationService;

    public CancellationController(InsuranceDbContext db, CancellationService cancellationService)
    {
        _db                  = db;
        _cancellationService = cancellationService;
    }

    // GET api/policies/5/cancellation/refund-preview
    [HttpGet("refund-preview")]
    public async Task<ActionResult<object>> RefundPreview(long policyId)
    {
        var policy = await _db.Policies
            .Include(p => p.Claims)
            .FirstOrDefaultAsync(p => p.Id == policyId);
        if (policy is null) return NotFound();

        double refund      = _cancellationService.CalculateRefundAmount(policy);
        bool inCoolingOff  = _cancellationService.IsWithinCoolingOffPeriod(policy);

        return Ok(new { policyId, withinCoolingOffPeriod = inCoolingOff, estimatedRefund = refund });
    }

    // POST api/policies/5/cancellation
    [HttpPost]
    public async Task<ActionResult<object>> Cancel(long policyId)
    {
        var policy = await _db.Policies
            .Include(p => p.Claims)
            .FirstOrDefaultAsync(p => p.Id == policyId);
        if (policy is null) return NotFound();

        _cancellationService.ValidateCancellationEligibility(policy);
        double refund = _cancellationService.CalculateRefundAmount(policy);

        policy.Status = PolicyStatus.Cancelled;
        await _db.SaveChangesAsync();

        return Ok(new { policyId, status = PolicyStatus.Cancelled, refundAmount = refund });
    }
}
