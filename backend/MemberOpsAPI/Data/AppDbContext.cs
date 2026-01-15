using Microsoft.EntityFrameworkCore;
using MemberOpsAPI.Models;

namespace MemberOpsAPI.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Staff> Staff { get; set; }
    public DbSet<Member> Members { get; set; }
    public DbSet<AccountFlag> AccountFlags { get; set; }
    public DbSet<ServiceRequest> ServiceRequests { get; set; }
    public DbSet<ServiceRequestComment> ServiceRequestComments { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure relationships
        modelBuilder.Entity<AccountFlag>()
            .HasOne(f => f.Member)
            .WithMany(m => m.Flags)
            .HasForeignKey(f => f.MemberId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AuditLog>()
            .HasOne(al => al.Member)
            .WithMany(m => m.AuditLogs)
            .HasForeignKey(al => al.MemberId)
            .OnDelete(DeleteBehavior.Cascade);

        // Unique constraints
        modelBuilder.Entity<Member>()
            .HasIndex(m => m.MemberNumber)
            .IsUnique();

        modelBuilder.Entity<Staff>()
            .HasIndex(s => s.Username)
            .IsUnique();

        // Service Request relationships
        modelBuilder.Entity<ServiceRequest>()
            .HasOne(sr => sr.Member)
            .WithMany(m => m.ServiceRequests)
            .HasForeignKey(sr => sr.MemberId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ServiceRequest>()
            .HasOne(sr => sr.CreatedBy)
            .WithMany()
            .HasForeignKey(sr => sr.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ServiceRequest>()
            .HasOne(sr => sr.AssignedTo)
            .WithMany()
            .HasForeignKey(sr => sr.AssignedToId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<ServiceRequest>()
            .HasOne(sr => sr.ResolvedBy)
            .WithMany()
            .HasForeignKey(sr => sr.ResolvedById)
            .OnDelete(DeleteBehavior.SetNull);

        // Service Request Comment relationships
        modelBuilder.Entity<ServiceRequestComment>()
            .HasOne(src => src.ServiceRequest)
            .WithMany(sr => sr.Comments)
            .HasForeignKey(src => src.ServiceRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ServiceRequestComment>()
            .HasOne(src => src.Staff)
            .WithMany()
            .HasForeignKey(src => src.StaffId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public async Task LogAuditAsync(int memberId, string actor, string action, string? details = null)
    {
        var auditLog = new AuditLog
        {
            MemberId = memberId,
            Actor = actor,
            Action = action,
            Details = details,
            Timestamp = DateTime.UtcNow
        };

        AuditLogs.Add(auditLog);
        await SaveChangesAsync();
    }

}
