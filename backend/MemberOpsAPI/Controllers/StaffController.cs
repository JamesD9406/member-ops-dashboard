using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MemberOpsAPI.Data;

namespace MemberOpsAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class StaffController : ControllerBase
{
    private readonly AppDbContext _context;

    public StaffController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetStaff()
    {
        var staff = await _context.Staff
            .Select(s => new
            {
                s.Id,
                s.Username,
                s.DisplayName,
                s.Email,
                s.Role
            })
            .OrderBy(s => s.DisplayName)
            .ToListAsync();

        return Ok(staff);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetStaffById(int id)
    {
        var staff = await _context.Staff
            .Where(s => s.Id == id)
            .Select(s => new
            {
                s.Id,
                s.Username,
                s.DisplayName,
                s.Email,
                s.Role
            })
            .FirstOrDefaultAsync();

        if (staff == null)
        {
            return NotFound(new { message = "Staff member not found" });
        }

        return Ok(staff);
    }
}
