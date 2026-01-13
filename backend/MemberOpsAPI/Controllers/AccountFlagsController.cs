using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MemberOpsAPI.Data;
using MemberOpsAPI.Models;

namespace MemberOpsAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/members/{memberId}/flags")]
public class AccountFlagsController : ControllerBase
{
    private readonly AppDbContext _context;

    public AccountFlagsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AccountFlag>>> GetMemberFlags(int memberId)
    {
        var member = await _context.Members.FindAsync(memberId);
        if (member == null)
        {
            return NotFound(new { message = "Member not found" });
        }

        var flags = await _context.AccountFlags
            .Where(f => f.MemberId == memberId)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();

        return Ok(flags);
    }

    [HttpPost]
    public async Task<ActionResult<AccountFlag>> CreateFlag(
        int memberId,
        [FromBody] CreateFlagRequest request)
    {
        var member = await _context.Members.FindAsync(memberId);
        if (member == null)
        {
            return NotFound(new { message = "Member not found" });
        }

        var username = User.Identity?.Name ?? "Unknown";

        var flag = new AccountFlag
        {
            MemberId = memberId,
            FlagType = request.FlagType,
            Description = request.Description,
            CreatedBy = username,
            CreatedAt = DateTime.UtcNow
        };

        _context.AccountFlags.Add(flag);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetMemberFlags), new { memberId }, flag);
    }

    [HttpPut("{flagId}/resolve")]
    public async Task<ActionResult<AccountFlag>> ResolveFlag(
        int memberId,
        int flagId,
        [FromBody] ResolveFlagRequest request)
    {
        var flag = await _context.AccountFlags
            .FirstOrDefaultAsync(f => f.Id == flagId && f.MemberId == memberId);

        if (flag == null)
        {
            return NotFound(new { message = "Flag not found" });
        }

        if (flag.ResolvedAt != null)
        {
            return BadRequest(new { message = "Flag is already resolved" });
        }

        var username = User.Identity?.Name ?? "Unknown";

        flag.ResolvedBy = username;
        flag.ResolvedAt = DateTime.UtcNow;
        flag.ResolutionNotes = request.ResolutionNotes;

        await _context.SaveChangesAsync();

        return Ok(flag);
    }
}

// DTOs for request bodies, small so they can stay here
public class CreateFlagRequest
{
    public required string FlagType { get; set; }
    public required string Description { get; set; }
}

public class ResolveFlagRequest
{
    public string? ResolutionNotes { get; set; }
}
