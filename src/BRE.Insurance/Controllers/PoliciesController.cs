using BRE.Insurance.Data;
using BRE.Insurance.Models;
using BRE.Insurance.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BRE.Insurance.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PoliciesController : ControllerBase
{
    private readonly InsuranceDbContext _db;
    private readonly PolicyService _policyService;
    private readonly KycService _kycService;

    public PoliciesController(InsuranceDbContext db, PolicyService policyService, KycService kycService)
    {
        _db            = db;
        _policyService = policyService;
        _kycService    = kycService;
    }

    // GET api/policies
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Policy>>> GetAll()
        => Ok(await _db.Policies.Include(p => p.User).ToListAsync());

    // GET api/policies/5
    [HttpGet("{id:long}")]
    public async Task<ActionResult<Policy>> GetById(long id)
    {
        var policy = await _db.Policies.Include(p => p.User).FirstOrDefaultAsync(p => p.Id == id);
        return policy is null ? NotFound() : Ok(policy);
    }

    // GET api/policies/users/5
    [HttpGet("users/{userId:long}")]
    public async Task<ActionResult<IEnumerable<Policy>>> GetByUser(long userId)
        => Ok(await _db.Policies.Where(p => p.UserId == userId).ToListAsync());

    // POST api/policies/users/5
    [HttpPost("users/{userId:long}")]
    public async Task<ActionResult<Policy>> Create(long userId, [FromBody] Policy policy)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user is null) return NotFound();

        _kycService.AssertKycVerified(user);
        _policyService.ValidateSumAssured(policy.SumAssured, policy.AnnualIncome);
        _policyService.ValidateDependentsCount(policy.DependentsCount);

        if (policy.PolicyCategory == PolicyCategory.Corporate)
            _policyService.ValidateCorporatePolicySize(policy.DependentsCount);

        if (policy.PolicyCategory == PolicyCategory.FamilyFloater)
            _policyService.ValidateFamilyFloaterMembers(policy.DependentsCount);

        policy.UserId       = userId;
        policy.User         = user;
        policy.PremiumAmount = _policyService.ApplySmokingLoading(policy.PremiumAmount, policy.IsSmoker);
        policy.Status       = PolicyStatus.Active;
        policy.PolicyNumber = $"POL-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
        policy.CreatedAt    = DateTime.UtcNow;

        _db.Policies.Add(policy);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = policy.Id }, policy);
    }

    // GET api/policies/5/medical-exam-required
    [HttpGet("{id:long}/medical-exam-required")]
    public async Task<ActionResult<object>> MedicalExamRequired(long id)
    {
        var policy = await _db.Policies.Include(p => p.User).FirstOrDefaultAsync(p => p.Id == id);
        if (policy is null || policy.User is null) return NotFound();

        bool required = _policyService.IsMedicalExamRequired(policy.User.DateOfBirth);
        return Ok(new { policyId = id, medicalExamRequired = required });
    }

    // GET api/policies/5/waiting-period-end
    [HttpGet("{id:long}/waiting-period-end")]
    public async Task<ActionResult<object>> WaitingPeriodEnd(long id)
    {
        var policy = await _db.Policies.FindAsync(id);
        if (policy is null) return NotFound();

        return Ok(new { policyId = id, waitingPeriodEnd = _policyService.CalculateWaitingPeriodEnd(policy) });
    }

    // DELETE api/policies/5
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        var policy = await _db.Policies.FindAsync(id);
        if (policy is null) return NotFound();

        _db.Policies.Remove(policy);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
