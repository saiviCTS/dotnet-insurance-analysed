using BRE.Insurance.Data;
using BRE.Insurance.Models;
using BRE.Insurance.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BRE.Insurance.Controllers;

[ApiController]
[Route("api/kyc")]
public class KycController : ControllerBase
{
    private readonly InsuranceDbContext _db;
    private readonly KycService _kycService;

    public KycController(InsuranceDbContext db, KycService kycService)
    {
        _db = db;
        _kycService = kycService;
    }

    // GET api/kyc/users/5/documents
    [HttpGet("users/{userId:long}/documents")]
    public async Task<ActionResult<IEnumerable<KycDocument>>> GetDocuments(long userId)
        => Ok(await _db.KycDocuments.Where(d => d.UserId == userId).ToListAsync());

    // POST api/kyc/users/5/documents
    [HttpPost("users/{userId:long}/documents")]
    public async Task<ActionResult<KycDocument>> UploadDocument(
        long userId, [FromBody] KycDocument document)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user is null) return NotFound();

        _kycService.IsDocumentValid(document);
        document.UserId = userId;
        document.User   = user;

        _db.KycDocuments.Add(document);
        await _db.SaveChangesAsync();
        return Ok(document);
    }

    // POST api/kyc/users/5/verify?success=true
    [HttpPost("users/{userId:long}/verify")]
    public async Task<ActionResult<object>> Verify(long userId, [FromQuery] bool success)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user is null) return NotFound();

        if (success)
        {
            user.KycVerified   = true;
            user.KycStatus     = KycStatus.Verified;
            user.KycVerifiedAt = DateTime.UtcNow;
        }
        else
        {
            _kycService.IncrementVerificationAttempt(user);
        }

        await _db.SaveChangesAsync();
        return Ok(new { userId, user.KycStatus, user.KycVerificationAttempts });
    }

    // GET api/kyc/users/5/status
    [HttpGet("users/{userId:long}/status")]
    public async Task<ActionResult<object>> GetStatus(long userId)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user is null) return NotFound();

        return Ok(new
        {
            userId,
            user.KycStatus,
            user.KycVerified,
            user.KycVerificationAttempts
        });
    }
}
