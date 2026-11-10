using BRE.Insurance.Data;
using BRE.Insurance.Models;
using BRE.Insurance.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BRE.Insurance.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly InsuranceDbContext _db;
    private readonly PaymentService _paymentService;

    public PaymentsController(InsuranceDbContext db, PaymentService paymentService)
    {
        _db             = db;
        _paymentService = paymentService;
    }

    // GET api/payments/5
    [HttpGet("{id:long}")]
    public async Task<ActionResult<Payment>> GetById(long id)
    {
        var payment = await _db.Payments.FindAsync(id);
        return payment is null ? NotFound() : Ok(payment);
    }

    // GET api/payments/policies/5
    [HttpGet("policies/{policyId:long}")]
    public async Task<ActionResult<IEnumerable<Payment>>> GetByPolicy(long policyId)
        => Ok(await _db.Payments.Where(p => p.PolicyId == policyId).ToListAsync());

    // POST api/payments/policies/5/initiate?amount=5000&channel=Upi
    [HttpPost("policies/{policyId:long}/initiate")]
    public async Task<ActionResult<Payment>> Initiate(
        long policyId, [FromQuery] double amount, [FromQuery] PaymentChannel channel)
    {
        var policy = await _db.Policies.FindAsync(policyId);
        if (policy is null) return NotFound();

        var payment = _paymentService.Initiate(amount, channel);
        payment.PolicyId      = policyId;
        payment.Policy        = policy;
        payment.TransactionId = $"TXN-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";

        _db.Payments.Add(payment);
        await _db.SaveChangesAsync();
        return Ok(payment);
    }

    // PATCH api/payments/5/complete?status=Success
    [HttpPatch("{id:long}/complete")]
    public async Task<ActionResult<Payment>> Complete(long id, [FromQuery] PaymentStatus status)
    {
        var payment = await _db.Payments.FindAsync(id);
        if (payment is null) return NotFound();

        if (!_paymentService.IsPaymentSessionValid(payment))
            return BadRequest("Payment session has expired.");

        if (status == PaymentStatus.Failed)
        {
            _paymentService.ValidateRetryEligibility(payment);
            payment.RetryCount++;
        }

        payment.Status      = status;
        payment.CompletedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Ok(payment);
    }

    // GET api/payments/fee-preview?amount=5000&channel=CreditCard
    [HttpGet("fee-preview")]
    public ActionResult<object> FeePreview([FromQuery] double amount, [FromQuery] PaymentChannel channel)
    {
        _paymentService.ValidatePaymentAmount(amount);
        double fee = _paymentService.CalculateConvenienceFee(amount, channel);
        return Ok(new { amount, channel, convenienceFee = fee, total = amount + fee });
    }
}
