using BRE.Insurance.Models;
using Microsoft.EntityFrameworkCore;

namespace BRE.Insurance.Data;

/// <summary>
/// EF Core DbContext — uses the in-memory provider (already in csproj).
/// Registered in Program.cs as a scoped service.
/// </summary>
public class InsuranceDbContext : DbContext
{
    public InsuranceDbContext(DbContextOptions<InsuranceDbContext> options) : base(options) { }

    public DbSet<User>          Users          => Set<User>();
    public DbSet<KycDocument>   KycDocuments   => Set<KycDocument>();
    public DbSet<Policy>        Policies       => Set<Policy>();
    public DbSet<Claim>         Claims         => Set<Claim>();
    public DbSet<ClaimDocument> ClaimDocuments => Set<ClaimDocument>();
    public DbSet<Payment>       Payments       => Set<Payment>();
    public DbSet<SupportTicket> SupportTickets => Set<SupportTicket>();
    public DbSet<Feedback>      Feedbacks      => Set<Feedback>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // User → KycDocuments
        modelBuilder.Entity<User>()
            .HasMany(u => u.KycDocuments)
            .WithOne(d => d.User)
            .HasForeignKey(d => d.UserId);

        // User → Policies
        modelBuilder.Entity<User>()
            .HasMany(u => u.Policies)
            .WithOne(p => p.User)
            .HasForeignKey(p => p.UserId);

        // Policy → Claims
        modelBuilder.Entity<Policy>()
            .HasMany(p => p.Claims)
            .WithOne(c => c.Policy)
            .HasForeignKey(c => c.PolicyId);

        // Policy → Payments
        modelBuilder.Entity<Policy>()
            .HasMany(p => p.Payments)
            .WithOne(pay => pay.Policy)
            .HasForeignKey(pay => pay.PolicyId);

        // Claim → ClaimDocuments
        modelBuilder.Entity<Claim>()
            .HasMany(c => c.Documents)
            .WithOne(d => d.Claim)
            .HasForeignKey(d => d.ClaimId);

        // User → SupportTickets
        modelBuilder.Entity<SupportTicket>()
            .HasOne(t => t.User)
            .WithMany()
            .HasForeignKey(t => t.UserId);

        // Feedback → User / Claim / Ticket
        modelBuilder.Entity<Feedback>()
            .HasOne(f => f.User)
            .WithMany()
            .HasForeignKey(f => f.UserId);

        modelBuilder.Entity<Feedback>()
            .HasOne(f => f.Claim)
            .WithMany()
            .HasForeignKey(f => f.ClaimId);

        modelBuilder.Entity<Feedback>()
            .HasOne(f => f.Ticket)
            .WithMany()
            .HasForeignKey(f => f.TicketId);
    }
}
