using BRE.Insurance.Data;
using BRE.Insurance.Models;
using BRE.Insurance.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BRE.Insurance.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly InsuranceDbContext _db;
    private readonly KycService _kycService;

    public UsersController(InsuranceDbContext db, KycService kycService)
    {
        _db = db;
        _kycService = kycService;
    }

    // GET api/users
    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetAll()
        => Ok(await _db.Users.ToListAsync());

    // GET api/users/5
    [HttpGet("{id:long}")]
    public async Task<ActionResult<User>> GetById(long id)
    {
        var user = await _db.Users.FindAsync(id);
        return user is null ? NotFound() : Ok(user);
    }

    // POST api/users
    [HttpPost]
    public async Task<ActionResult<User>> Create([FromBody] User user)
    {
        _kycService.IsAgeEligible(user.DateOfBirth);

        user.KycStatus               = KycStatus.Pending;
        user.KycVerified             = false;
        user.KycVerificationAttempts = 0;
        user.CreatedAt               = DateTime.UtcNow;

        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
    }

    // PUT api/users/5
    [HttpPut("{id:long}")]
    public async Task<ActionResult<User>> Update(long id, [FromBody] User updated)
    {
        var user = await _db.Users.FindAsync(id);
        if (user is null) return NotFound();

        user.FullName     = updated.FullName;
        user.Address      = updated.Address;
        user.City         = updated.City;
        user.State        = updated.State;
        user.Pincode      = updated.Pincode;
        user.MobileNumber = updated.MobileNumber;

        await _db.SaveChangesAsync();
        return Ok(user);
    }

    // DELETE api/users/5
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user is null) return NotFound();

        _db.Users.Remove(user);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
