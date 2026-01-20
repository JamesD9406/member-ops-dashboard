using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MemberOpsAPI.Controllers;
using MemberOpsAPI.Data;
using MemberOpsAPI.Models;
using System.Security.Claims;

namespace MemberOpsAPI.Tests.Controllers;

public class MembersControllerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly MembersController _controller;

    public MembersControllerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _controller = new MembersController(_context);

        SeedTestData();
    }

    private void SeedTestData()
    {
        var members = new List<Member>
        {
            new Member
            {
                Id = 1,
                MemberNumber = "M-100001",
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                Phone = "555-0101",
                Status = "Active",
                JoinDate = DateTime.UtcNow.AddYears(-2)
            },
            new Member
            {
                Id = 2,
                MemberNumber = "M-100002",
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@example.com",
                Phone = "555-0102",
                Status = "Locked",
                JoinDate = DateTime.UtcNow.AddYears(-1)
            },
            new Member
            {
                Id = 3,
                MemberNumber = "M-100003",
                FirstName = "Bob",
                LastName = "Johnson",
                Email = "bob.johnson@example.com",
                Phone = "555-0103",
                Status = "Active",
                JoinDate = DateTime.UtcNow.AddMonths(-6)
            }
        };

        _context.Members.AddRange(members);
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
    public async Task GetMembers_ReturnsAllMembers()
    {
        var result = await _controller.GetMembers();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var members = Assert.IsAssignableFrom<IEnumerable<Member>>(okResult.Value);
        Assert.Equal(3, members.Count());
    }

    [Fact]
    public async Task GetMembers_WithSearchTerm_ReturnsFilteredMembers()
    {
        var result = await _controller.GetMembers(search: "John");

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var members = Assert.IsAssignableFrom<IEnumerable<Member>>(okResult.Value);
        Assert.Equal(2, members.Count()); // John Doe and Bob Johnson
    }

    [Fact]
    public async Task GetMembers_WithStatusFilter_ReturnsFilteredMembers()
    {
        var result = await _controller.GetMembers(status: "Locked");

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var members = Assert.IsAssignableFrom<IEnumerable<Member>>(okResult.Value);
        Assert.Single(members);
        Assert.Equal("Jane", members.First().FirstName);
    }

    [Fact]
    public async Task GetMember_WithValidId_ReturnsMember()
    {
        var result = await _controller.GetMember(1);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var member = Assert.IsType<Member>(okResult.Value);
        Assert.Equal("John", member.FirstName);
        Assert.Equal("M-100001", member.MemberNumber);
    }

    [Fact]
    public async Task GetMember_WithInvalidId_ReturnsNotFound()
    {
        var result = await _controller.GetMember(999);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task LockMember_WithValidId_LocksMemberAndCreatesAuditLog()
    {
        SetupUserContext("supervisor", "Supervisor");

        var result = await _controller.LockMember(1);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var member = Assert.IsType<Member>(okResult.Value);
        Assert.Equal("Locked", member.Status);

        var auditLog = await _context.AuditLogs.FirstOrDefaultAsync(a => a.MemberId == 1);
        Assert.NotNull(auditLog);
        Assert.Equal("supervisor", auditLog.Actor);
        Assert.Contains("locked", auditLog.Action.ToLower());
    }

    [Fact]
    public async Task LockMember_AlreadyLocked_ReturnsBadRequest()
    {
        SetupUserContext("supervisor", "Supervisor");

        var result = await _controller.LockMember(2); // Jane is already locked

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task LockMember_WithInvalidId_ReturnsNotFound()
    {
        SetupUserContext("supervisor", "Supervisor");

        var result = await _controller.LockMember(999);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task UnlockMember_WithLockedMember_UnlocksMemberAndCreatesAuditLog()
    {
        SetupUserContext("admin", "Admin");

        var result = await _controller.UnlockMember(2); // Jane is locked

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var member = Assert.IsType<Member>(okResult.Value);
        Assert.Equal("Active", member.Status);

        var auditLog = await _context.AuditLogs.FirstOrDefaultAsync(a => a.MemberId == 2);
        Assert.NotNull(auditLog);
        Assert.Equal("admin", auditLog.Actor);
        Assert.Contains("unlocked", auditLog.Action.ToLower());
    }

    [Fact]
    public async Task UnlockMember_NotLocked_ReturnsBadRequest()
    {
        SetupUserContext("supervisor", "Supervisor");

        var result = await _controller.UnlockMember(1); // John is Active

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task UpdateNotes_WithValidId_UpdatesNotesAndCreatesAuditLog()
    {
        SetupUserContext("supervisor", "Supervisor");
        var request = new UpdateNotesRequest { Notes = "Updated member notes" };

        var result = await _controller.UpdateNotes(1, request);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var member = Assert.IsType<Member>(okResult.Value);
        Assert.Equal("Updated member notes", member.Notes);

        var auditLog = await _context.AuditLogs.FirstOrDefaultAsync(a => a.MemberId == 1);
        Assert.NotNull(auditLog);
        Assert.Contains("notes", auditLog.Action.ToLower());
    }

    [Fact]
    public async Task UpdateNotes_WithInvalidId_ReturnsNotFound()
    {
        SetupUserContext("supervisor", "Supervisor");
        var request = new UpdateNotesRequest { Notes = "Some notes" };

        var result = await _controller.UpdateNotes(999, request);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }
}
