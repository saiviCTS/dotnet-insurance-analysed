using BRE.Insurance.Data;
using BRE.Insurance.Rules;
using BRE.Insurance.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// In-memory EF Core database
builder.Services.AddDbContext<InsuranceDbContext>(options =>
    options.UseInMemoryDatabase("InsuranceDb"));

builder.Services.AddControllers();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie();
builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Insurance BRE POC (.NET 9)", Version = "v1" });
});

// Register all services
builder.Services.AddScoped<KycService>();
builder.Services.AddScoped<PolicyService>();
builder.Services.AddScoped<PaymentService>();
builder.Services.AddScoped<ClaimService>();
builder.Services.AddScoped<CancellationService>();
builder.Services.AddScoped<FeedbackService>();
builder.Services.AddScoped<BusinessRuleExtractor>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// Return 400 for business rule violations instead of 500
app.UseExceptionHandler(errApp => errApp.Run(async ctx =>
{
    var ex = ctx.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;
    ctx.Response.ContentType = "application/json";
    ctx.Response.StatusCode  = ex is ArgumentException or InvalidOperationException ? 400 : 500;
    await ctx.Response.WriteAsJsonAsync(new { error = ex?.Message });
}));

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Print BRE catalog on startup for demo
var extractor = app.Services.GetRequiredService<BusinessRuleExtractor>();
extractor.PrintRulesCatalog();

app.Run();
