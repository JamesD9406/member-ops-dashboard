using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MemberOpsAPI.Controllers;
using MemberOpsAPI.Data;
using MemberOpsAPI.Models;
using System.Security.Claims;

namespace MemberOpsAPI.Tests.Authorization;

public class RoleAuthorizationTests : IDisposable
{
    private readonly AppDbContext _context;

    public RoleAuthorizationTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
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

        var flag = new AccountFlag
        {
            Id = 1,
            MemberId = 1,
            FlagType = "FraudReview",
            Description = "Test flag",
            CreatedBy = "agent",
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        _context.Members.Add(member);
        _context.AccountFlags.Add(flag);
        _context.SaveChanges();
    }

    private T CreateControllerWithRole<T>(string username, string role) where T : ControllerBase
    {
        T controller;

        if (typeof(T) == typeof(MembersController))
            controller = (T)(ControllerBase)new MembersController(_context);
        else if (typeof(T) == typeof(AccountFlagsController))
            controller = (T)(ControllerBase)new AccountFlagsController(_context);
        else
            throw new ArgumentException($"Unsupported controller type: {typeof(T).Name}");

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };

        return controller;
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Theory]
    [InlineData("Supervisor")]
    [InlineData("Admin")]
    public async Task LockMember_SupervisorAndAdmin_CanLockAccounts(string role)
    {
        var controller = CreateControllerWithRole<MembersController>("testuser", role);

        var result = await controller.LockMember(1);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var member = Assert.IsType<Member>(okResult.Value);
        Assert.Equal("Locked", member.Status);
    }

    [Theory]
    [InlineData("Supervisor")]
    [InlineData("Admin")]
    public async Task UnlockMember_SupervisorAndAdmin_CanUnlockAccounts(string role)
    {
        var member = await _context.Members.FindAsync(1);
        member!.Status = "Locked";
        await _context.SaveChangesAsync();

        var controller = CreateControllerWithRole<MembersController>("testuser", role);

        var result = await controller.UnlockMember(1);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var updatedMember = Assert.IsType<Member>(okResult.Value);
        Assert.Equal("Active", updatedMember.Status);
    }

    [Theory]
    [InlineData("Supervisor")]
    [InlineData("Admin")]
    public async Task UpdateNotes_SupervisorAndAdmin_CanEditNotes(string role)
    {
        var controller = CreateControllerWithRole<MembersController>("testuser", role);
        var request = new UpdateNotesRequest { Notes = "Updated by " + role };

        var result = await controller.UpdateNotes(1, request);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var member = Assert.IsType<Member>(okResult.Value);
        Assert.Equal("Updated by " + role, member.Notes);
    }

    [Theory]
    [InlineData("Supervisor")]
    [InlineData("Admin")]
    public async Task ResolveFlag_SupervisorAndAdmin_CanResolveFlags(string role)
    {
        var controller = CreateControllerWithRole<AccountFlagsController>("testuser", role);
        var request = new ResolveFlagRequest { ResolutionNotes = "Resolved by " + role };

        var result = await controller.ResolveFlag(1, 1, request);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var flag = Assert.IsType<AccountFlag>(okResult.Value);
        Assert.NotNull(flag.ResolvedAt);
        Assert.Equal("testuser", flag.ResolvedBy);
    }

    [Theory]
    [InlineData("Agent")]
    [InlineData("Supervisor")]
    [InlineData("Admin")]
    public async Task CreateFlag_AllRoles_CanCreateFlags(string role)
    {
        var controller = CreateControllerWithRole<AccountFlagsController>("testuser", role);
        var request = new CreateFlagRequest
        {
            FlagType = "GeneralReview",
            Description = "Created by " + role
        };

        var result = await controller.CreateFlag(1, request);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var flag = Assert.IsType<AccountFlag>(createdResult.Value);
        Assert.Equal("testuser", flag.CreatedBy);
    }

    [Theory]
    [InlineData("Agent")]
    [InlineData("Supervisor")]
    [InlineData("Admin")]
    public async Task GetMembers_AllRoles_CanViewMembers(string role)
    {
        var controller = CreateControllerWithRole<MembersController>("testuser", role);

        var result = await controller.GetMembers();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var members = Assert.IsAssignableFrom<IEnumerable<Member>>(okResult.Value);
        Assert.NotEmpty(members);
    }

    [Theory]
    [InlineData("Agent")]
    [InlineData("Supervisor")]
    [InlineData("Admin")]
    public async Task GetMemberFlags_AllRoles_CanViewFlags(string role)
    {
        var controller = CreateControllerWithRole<AccountFlagsController>("testuser", role);

        var result = await controller.GetMemberFlags(1);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var flags = Assert.IsAssignableFrom<IEnumerable<AccountFlag>>(okResult.Value);
        Assert.NotEmpty(flags);
    }
}
