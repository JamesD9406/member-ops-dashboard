using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MemberOpsAPI.Controllers;
using MemberOpsAPI.Data;
using MemberOpsAPI.Models;
using System.Security.Claims;

namespace MemberOpsAPI.Tests.Controllers;

public class AccountFlagsControllerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly AccountFlagsController _controller;

    public AccountFlagsControllerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _controller = new AccountFlagsController(_context);

        SeedTestData();
    }

    private void SeedTestData()
    {
        var member = new Member
        {
            Id = 1,
            MemberNumber = "M-100001",
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Phone = "555-0100",
            Status = "Active",
            JoinDate = DateTime.UtcNow.AddYears(-1)
        };

        var flags = new List<AccountFlag>
        {
            new AccountFlag
            {
                Id = 1,
                MemberId = 1,
                FlagType = "FraudReview",
                Description = "Suspicious activity detected",
                CreatedBy = "agent1",
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            },
            new AccountFlag
            {
                Id = 2,
                MemberId = 1,
                FlagType = "IDVerification",
                Description = "ID verification required",
                CreatedBy = "agent2",
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                ResolvedBy = "supervisor1",
                ResolvedAt = DateTime.UtcNow.AddDays(-1),
                ResolutionNotes = "ID verified in branch"
            }
        };

        _context.Members.Add(member);
        _context.AccountFlags.AddRange(flags);
        _context.SaveChanges();
    }

    private void SetupUserContext(string username, string role)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task GetMemberFlags_WithValidMemberId_ReturnsFlags()
    {
        var result = await _controller.GetMemberFlags(1);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var flags = Assert.IsAssignableFrom<IEnumerable<AccountFlag>>(okResult.Value);
        Assert.Equal(2, flags.Count());
    }

    [Fact]
    public async Task GetMemberFlags_WithInvalidMemberId_ReturnsNotFound()
    {
        var result = await _controller.GetMemberFlags(999);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task CreateFlag_WithValidData_CreatesFlagAndAuditLog()
    {
        SetupUserContext("agent1", "Staff");
        var request = new CreateFlagRequest
        {
            FlagType = "PaymentIssue",
            Description = "Missed payment on loan"
        };

        var result = await _controller.CreateFlag(1, request);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var flag = Assert.IsType<AccountFlag>(createdResult.Value);
        Assert.Equal("PaymentIssue", flag.FlagType);
        Assert.Equal("Missed payment on loan", flag.Description);
        Assert.Equal("agent1", flag.CreatedBy);
        Assert.Null(flag.ResolvedAt);

        var auditLog = await _context.AuditLogs.FirstOrDefaultAsync(a => a.MemberId == 1);
        Assert.NotNull(auditLog);
        Assert.Contains("PaymentIssue", auditLog.Details);
    }

    [Fact]
    public async Task CreateFlag_WithInvalidMemberId_ReturnsNotFound()
    {
        SetupUserContext("agent1", "Staff");
        var request = new CreateFlagRequest
        {
            FlagType = "GeneralReview",
            Description = "General review needed"
        };

        var result = await _controller.CreateFlag(999, request);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task ResolveFlag_AsSupervisor_ResolvesFlagAndCreatesAuditLog()
    {
        SetupUserContext("supervisor1", "Supervisor");
        var request = new ResolveFlagRequest
        {
            ResolutionNotes = "Issue investigated and resolved"
        };

        var result = await _controller.ResolveFlag(1, 1, request);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var flag = Assert.IsType<AccountFlag>(okResult.Value);
        Assert.Equal("supervisor1", flag.ResolvedBy);
        Assert.NotNull(flag.ResolvedAt);
        Assert.Equal("Issue investigated and resolved", flag.ResolutionNotes);

        var auditLog = await _context.AuditLogs
            .OrderByDescending(a => a.Timestamp)
            .FirstOrDefaultAsync(a => a.MemberId == 1);
        Assert.NotNull(auditLog);
        Assert.Contains("resolved", auditLog.Action.ToLower());
    }

    [Fact]
    public async Task ResolveFlag_AsAdmin_ResolvesFlagSuccessfully()
    {
        SetupUserContext("admin1", "Admin");
        var request = new ResolveFlagRequest
        {
            ResolutionNotes = "Resolved by admin"
        };

        var result = await _controller.ResolveFlag(1, 1, request);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var flag = Assert.IsType<AccountFlag>(okResult.Value);
        Assert.Equal("admin1", flag.ResolvedBy);
        Assert.NotNull(flag.ResolvedAt);
    }

    [Fact]
    public async Task ResolveFlag_AlreadyResolved_ReturnsBadRequest()
    {
        SetupUserContext("supervisor1", "Supervisor");
        var request = new ResolveFlagRequest
        {
            ResolutionNotes = "Trying to resolve again"
        };

        var result = await _controller.ResolveFlag(1, 2, request); // Flag 2 is already resolved

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task ResolveFlag_WithInvalidFlagId_ReturnsNotFound()
    {
        SetupUserContext("supervisor1", "Supervisor");
        var request = new ResolveFlagRequest
        {
            ResolutionNotes = "Some notes"
        };

        var result = await _controller.ResolveFlag(1, 999, request);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task ResolveFlag_WithoutResolutionNotes_StillResolvesFlag()
    {
        SetupUserContext("supervisor1", "Supervisor");
        var request = new ResolveFlagRequest
        {
            ResolutionNotes = null
        };

        var result = await _controller.ResolveFlag(1, 1, request);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var flag = Assert.IsType<AccountFlag>(okResult.Value);
        Assert.NotNull(flag.ResolvedAt);
        Assert.Null(flag.ResolutionNotes);
    }

    [Theory]
    [InlineData("FraudReview")]
    [InlineData("IDVerification")]
    [InlineData("PaymentIssue")]
    [InlineData("GeneralReview")]
    public async Task CreateFlag_WithDifferentFlagTypes_CreatesCorrectFlagType(string flagType)
    {
        SetupUserContext("agent1", "Staff");
        var request = new CreateFlagRequest
        {
            FlagType = flagType,
            Description = $"Test {flagType} flag"
        };

        var result = await _controller.CreateFlag(1, request);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var flag = Assert.IsType<AccountFlag>(createdResult.Value);
        Assert.Equal(flagType, flag.FlagType);
    }
}
