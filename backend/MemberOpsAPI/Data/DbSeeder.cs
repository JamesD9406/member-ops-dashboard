using MemberOpsAPI.Models;
using MemberOpsAPI.Constants;

namespace MemberOpsAPI.Data;

public static class DbSeeder
{
    public static void SeedData(AppDbContext context)
    {
        // Check if data already exists
        if (context.Staff.Any() || context.Members.Any())
        {
            return;
        }

        // Seed Staff with BCrypt hashed passwords
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
                Username = "staff",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Staff123!"),
                DisplayName = "Sam Staff",
                Email = "staff@memberops.local",
                Role = "Staff"
            }
        };
        context.Staff.AddRange(staff);
        context.SaveChanges();

        // Seed Members (rest of the code remains the same)
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
        context.Members.AddRange(members);
        context.SaveChanges();

        // Seed Account Flags (unchanged)
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
                CreatedBy = "staff",
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new AccountFlag
            {
                MemberId = members[6].Id,
                FlagType = "PaymentIssue",
                Description = "Missed payment - contacted member",
                CreatedBy = "staff",
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                ResolvedBy = "supervisor",
                ResolvedAt = DateTime.UtcNow.AddHours(-2),
                ResolutionNotes = "Payment received, flag resolved"
            }
        };
        context.AccountFlags.AddRange(flags);
        context.SaveChanges();

        // Seed Service Requests (unchanged)
        var serviceRequests = new List<ServiceRequest>
        {
            new ServiceRequest
            {
                MemberId = members[0].Id,
                RequestType = "CardReplacement",
                Description = "Lost card, needs replacement",
                Status = "InProgress",
                CreatedBy = "staff",
                CreatedAt = DateTime.UtcNow.AddDays(-3)
            },
            new ServiceRequest
            {
                MemberId = members[3].Id,
                RequestType = "StatementRequest",
                Description = "Needs last 6 months statements for tax purposes",
                Status = "Open",
                CreatedBy = "staff",
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new ServiceRequest
            {
                MemberId = members[1].Id,
                RequestType = "AddressChange",
                Description = "Moving to new address, update records",
                Status = "Completed",
                CreatedBy = "staff",
                CreatedAt = DateTime.UtcNow.AddDays(-7),
                CompletedBy = "supervisor",
                CompletedAt = DateTime.UtcNow.AddDays(-6)
            },
            new ServiceRequest
            {
                MemberId = members[4].Id,
                RequestType = "Question",
                Description = "Question about overdraft protection options",
                Status = "Open",
                CreatedBy = "staff",
                CreatedAt = DateTime.UtcNow.AddHours(-4)
            }
        };
        context.ServiceRequests.AddRange(serviceRequests);
        context.SaveChanges();

        // Seed Audit Logs
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
                Actor = "staff",
                Action = AuditActions.ServiceRequestCreated,
                Details = "Type: CardReplacement",
                Timestamp = DateTime.UtcNow.AddDays(-3)
            }
        };
        context.AuditLogs.AddRange(auditLogs);
        context.SaveChanges();
    }
}
