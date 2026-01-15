using MemberOpsAPI.Models;
using MemberOpsAPI.Constants;
using Microsoft.EntityFrameworkCore;

namespace MemberOpsAPI.Data;

public static class DbSeeder
{
    // Main orchestrator method
    public static async Task SeedAsync(AppDbContext context)
    {
        await SeedStaffAsync(context);
        await SeedMembersAsync(context);
        await SeedAccountFlagsAsync(context);
        await SeedServiceRequestsAsync(context);
        await SeedAuditLogsAsync(context);
    }

    private static async Task SeedStaffAsync(AppDbContext context)
    {
        if (await context.Staff.AnyAsync())
            return;

        var staff = new List<Staff>
        {
            new Staff
            {
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                DisplayName = "Admin User",
                Email = "admin@memberops.local",
                Role = "Admin"
            },
            new Staff
            {
                Username = "supervisor",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Super123!"),
                DisplayName = "Sarah Supervisor",
                Email = "supervisor@memberops.local",
                Role = "Supervisor"
            },
            new Staff
            {
                Username = "agent",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Agent123!"),
                DisplayName = "Sam Agent",
                Email = "agent@memberops.local",
                Role = "Agent"
            }
        };

        await context.Staff.AddRangeAsync(staff);
        await context.SaveChangesAsync();
    }

    private static async Task SeedMembersAsync(AppDbContext context)
    {
        if (await context.Members.AnyAsync())
            return;

        var members = new List<Member>
        {
            new Member
            {
                MemberNumber = "M-100001",
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@email.com",
                Phone = "519-555-0101",
                Status = "Active",
                JoinDate = DateTime.UtcNow.AddYears(-5)
            },
            new Member
            {
                MemberNumber = "M-100002",
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@email.com",
                Phone = "519-555-0102",
                Status = "Active",
                JoinDate = DateTime.UtcNow.AddYears(-3),
                Notes = "Preferred contact method: email"
            },
            new Member
            {
                MemberNumber = "M-100003",
                FirstName = "Robert",
                LastName = "Johnson",
                Email = "robert.j@email.com",
                Phone = "519-555-0103",
                Status = "Locked",
                JoinDate = DateTime.UtcNow.AddYears(-2),
                Notes = "Account locked pending ID verification"
            },
            new Member
            {
                MemberNumber = "M-100004",
                FirstName = "Emily",
                LastName = "Davis",
                Email = "emily.davis@email.com",
                Phone = "519-555-0104",
                Status = "Active",
                JoinDate = DateTime.UtcNow.AddYears(-1)
            },
            new Member
            {
                MemberNumber = "M-100005",
                FirstName = "Michael",
                LastName = "Wilson",
                Email = "m.wilson@email.com",
                Phone = "519-555-0105",
                Status = "Active",
                JoinDate = DateTime.UtcNow.AddMonths(-6)
            },
            new Member
            {
                MemberNumber = "M-100006",
                FirstName = "Sarah",
                LastName = "Brown",
                Email = "sarah.brown@email.com",
                Phone = "519-555-0106",
                Status = "Closed",
                JoinDate = DateTime.UtcNow.AddYears(-4),
                Notes = "Account closed at member request"
            },
            new Member
            {
                MemberNumber = "M-100007",
                FirstName = "David",
                LastName = "Martinez",
                Email = "david.m@email.com",
                Phone = "519-555-0107",
                Status = "Active",
                JoinDate = DateTime.UtcNow.AddYears(-7)
            },
            new Member
            {
                MemberNumber = "M-100008",
                FirstName = "Lisa",
                LastName = "Anderson",
                Email = "lisa.anderson@email.com",
                Phone = "519-555-0108",
                Status = "Active",
                JoinDate = DateTime.UtcNow.AddMonths(-3)
            }
        };

        await context.Members.AddRangeAsync(members);
        await context.SaveChangesAsync();
    }

    private static async Task SeedAccountFlagsAsync(AppDbContext context)
    {
        if (await context.AccountFlags.AnyAsync())
            return;

        var members = await context.Members.ToListAsync();

        var flags = new List<AccountFlag>
        {
            new AccountFlag
            {
                MemberId = members[2].Id,
                FlagType = "IDVerification",
                Description = "Driver's license expired, needs updated ID",
                CreatedBy = "supervisor",
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            },
            new AccountFlag
            {
                MemberId = members[1].Id,
                FlagType = "GeneralReview",
                Description = "Requested credit limit increase review",
                CreatedBy = "agent",
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new AccountFlag
            {
                MemberId = members[6].Id,
                FlagType = "PaymentIssue",
                Description = "Missed payment - contacted member",
                CreatedBy = "agent",
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                ResolvedBy = "supervisor",
                ResolvedAt = DateTime.UtcNow.AddHours(-2),
                ResolutionNotes = "Payment received, flag resolved"
            }
        };

        await context.AccountFlags.AddRangeAsync(flags);
        await context.SaveChangesAsync();
    }

    private static async Task SeedServiceRequestsAsync(AppDbContext context)
    {
        if (await context.ServiceRequests.AnyAsync())
            return;

        var members = await context.Members.ToListAsync();
        var staff = await context.Staff.ToListAsync();
        var admin = staff.First(s => s.Username == "admin");
        var supervisor = staff.First(s => s.Username == "supervisor");
        var agent = staff.First(s => s.Username == "agent");

        var serviceRequests = new List<ServiceRequest>
        {
            // New request - not assigned yet
            new ServiceRequest
            {
                MemberId = members[0].Id,
                RequestType = "Account Inquiry",
                Description = "Member is unable to access their account dashboard. Getting error message 'Session Expired'.",
                Status = ServiceRequestStatus.New,
                Priority = ServiceRequestPriority.High,
                CreatedById = agent.Id,
                CreatedAt = DateTime.UtcNow.AddHours(-2)
            },

            // In Progress - assigned to agent
            new ServiceRequest
            {
                MemberId = members[1].Id,
                RequestType = "Technical Support",
                Description = "Member reports mobile app crashes when trying to upload documents.",
                Status = ServiceRequestStatus.InProgress,
                Priority = ServiceRequestPriority.Medium,
                CreatedById = supervisor.Id,
                AssignedToId = agent.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddHours(-3)
            },

            // Resolved - completed successfully
            new ServiceRequest
            {
                MemberId = members[2].Id,
                RequestType = "Billing",
                Description = "Member requesting refund for duplicate charge on account.",
                Status = ServiceRequestStatus.Resolved,
                Priority = ServiceRequestPriority.High,
                CreatedById = agent.Id,
                AssignedToId = supervisor.Id,
                ResolvedById = supervisor.Id,
                ResolutionType = ResolutionType.Resolved,
                ResolutionNotes = "Duplicate charge confirmed. Refund processed and will appear in 3-5 business days.",
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                UpdatedAt = DateTime.UtcNow.AddDays(-2),
                ResolvedAt = DateTime.UtcNow.AddDays(-2)
            },

            // Resolved - more info needed
            new ServiceRequest
            {
                MemberId = members[3].Id,
                RequestType = "Account Inquiry",
                Description = "Member claims unauthorized access to account.",
                Status = ServiceRequestStatus.Resolved,
                Priority = ServiceRequestPriority.Urgent,
                CreatedById = supervisor.Id,
                AssignedToId = agent.Id,
                ResolvedById = agent.Id,
                ResolutionType = ResolutionType.MoreInfoNeeded,
                ResolutionNotes = "Initial investigation shows normal login patterns. Requested member provide specific dates/times of suspected unauthorized access.",
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                UpdatedAt = DateTime.UtcNow.AddDays(-4),
                ResolvedAt = DateTime.UtcNow.AddDays(-4)
            },

            // In Progress - urgent priority
            new ServiceRequest
            {
                MemberId = members[4].Id,
                RequestType = "Account Locked",
                Description = "Member account locked after multiple failed login attempts. Member verified identity via phone.",
                Status = ServiceRequestStatus.InProgress,
                Priority = ServiceRequestPriority.Urgent,
                CreatedById = agent.Id,
                AssignedToId = supervisor.Id,
                CreatedAt = DateTime.UtcNow.AddMinutes(-30),
                UpdatedAt = DateTime.UtcNow.AddMinutes(-25)
            },

            // Resolved - transferred
            new ServiceRequest
            {
                MemberId = members[5].Id,
                RequestType = "Technical Support",
                Description = "Member reporting issues with payment gateway integration on business account.",
                Status = ServiceRequestStatus.Resolved,
                Priority = ServiceRequestPriority.Medium,
                CreatedById = agent.Id,
                AssignedToId = supervisor.Id,
                ResolvedById = supervisor.Id,
                ResolutionType = ResolutionType.Transferred,
                ResolutionNotes = "Issue requires developer investigation. Transferred to Technical Team via ticket #TK-2847.",
                CreatedAt = DateTime.UtcNow.AddDays(-7),
                UpdatedAt = DateTime.UtcNow.AddDays(-6),
                ResolvedAt = DateTime.UtcNow.AddDays(-6)
            },

            // New - low priority
            new ServiceRequest
            {
                MemberId = members[6].Id,
                RequestType = "General Inquiry",
                Description = "Member asking about upcoming feature releases and product roadmap.",
                Status = ServiceRequestStatus.New,
                Priority = ServiceRequestPriority.Low,
                CreatedById = agent.Id,
                CreatedAt = DateTime.UtcNow.AddHours(-5)
            }
        };

        await context.ServiceRequests.AddRangeAsync(serviceRequests);
        await context.SaveChangesAsync();

        // Add some comments to the In Progress request
        var inProgressRequest = serviceRequests.First(sr => sr.Status == ServiceRequestStatus.InProgress && sr.RequestType == "Technical Support");
        var comments = new List<ServiceRequestComment>
        {
            new ServiceRequestComment
            {
                ServiceRequestId = inProgressRequest.Id,
                StaffId = agent.Id,
                CommentText = "Starting investigation. Requested member's device information and app version.",
                CreatedAt = DateTime.UtcNow.AddHours(-2)
            },
            new ServiceRequestComment
            {
                ServiceRequestId = inProgressRequest.Id,
                StaffId = agent.Id,
                CommentText = "Member is using iOS 16.5 with app version 2.3.1. Unable to reproduce issue on test device.",
                CreatedAt = DateTime.UtcNow.AddMinutes(-45)
            }
        };

        await context.ServiceRequestComments.AddRangeAsync(comments);
        await context.SaveChangesAsync();
    }

    private static async Task SeedAuditLogsAsync(AppDbContext context)
    {
        if (await context.AuditLogs.AnyAsync())
            return;

        var members = await context.Members.ToListAsync();

        var auditLogs = new List<AuditLog>
        {
            new AuditLog
            {
                MemberId = members[2].Id,
                Actor = "supervisor",
                Action = AuditActions.AccountLocked,
                Details = "Account locked due to ID verification requirement",
                Timestamp = DateTime.UtcNow.AddDays(-5)
            },
            new AuditLog
            {
                MemberId = members[2].Id,
                Actor = "supervisor",
                Action = AuditActions.FlagCreated,
                Details = "Flag Type: IDVerification",
                Timestamp = DateTime.UtcNow.AddDays(-5)
            },
            new AuditLog
            {
                MemberId = members[1].Id,
                Actor = "supervisor",
                Action = AuditActions.NotesUpdated,
                Details = "Added preferred contact method",
                Timestamp = DateTime.UtcNow.AddDays(-10)
            },
            new AuditLog
            {
                MemberId = members[6].Id,
                Actor = "supervisor",
                Action = AuditActions.FlagResolved,
                Details = "PaymentIssue flag resolved - payment received",
                Timestamp = DateTime.UtcNow.AddHours(-2)
            },
            new AuditLog
            {
                MemberId = members[0].Id,
                Actor = "agent",
                Action = ServiceRequestActions.RequestCreated,
                Details = "Type: Account Inquiry",
                Timestamp = DateTime.UtcNow.AddHours(-2)
            }
        };

        await context.AuditLogs.AddRangeAsync(auditLogs);
        await context.SaveChangesAsync();
    }
}
