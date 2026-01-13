using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MemberOpsAPI.Data;
using MemberOpsAPI.Models;

namespace MemberOpsAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MembersController : ControllerBase
{
    private readonly AppDbContext _context;

    public MembersController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Member>>> GetMembers(
        [FromQuery] string? search = null,
        [FromQuery] string? status = null)
    {
        var query = _context.Members.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(m =>
                m.MemberNumber.ToLower().Contains(searchLower) ||
                m.FirstName.ToLower().Contains(searchLower) ||
                m.LastName.ToLower().Contains(searchLower) ||
                m.Email.ToLower().Contains(searchLower) ||
                (m.Phone != null && m.Phone.ToLower().Contains(searchLower))
            );
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(m => m.Status == status);
        }

        var members = await query
            .Include(m => m.Flags)
            .OrderByDescending(m => m.JoinDate)
            .ToListAsync();

        return Ok(members);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Member>> GetMember(int id)
    {
        var member = await _context.Members
            .Include(m => m.Flags)
            .Include(m => m.ServiceRequests)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (member == null)
        {
            return NotFound(new { message = "Member not found" });
        }

        return Ok(member);
    }
}
