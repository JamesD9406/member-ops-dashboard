using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MemberOpsAPI.Data;
using MemberOpsAPI.Models;

namespace MemberOpsAPI.Controllers;

[Authorize(Roles = "Supervisor,Admin")]
[ApiController]
[Route("api/audit-log")]
public class AuditLogController : ControllerBase
{
    private readonly AppDbContext _context;

    public AuditLogController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AuditLog>>> GetAuditLogs(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] int? memberId,
        [FromQuery] string? action,
        [FromQuery] string? actor)
    {
        var query = _context.AuditLogs
            .Include(a => a.Member)
            .AsQueryable();

        if (startDate.HasValue)
        {
            // Convert to UTC and start of day
            var startUtc = DateTime.SpecifyKind(startDate.Value.Date, DateTimeKind.Utc);
            query = query.Where(a => a.Timestamp >= startUtc);
        }

        if (endDate.HasValue)
        {
            // Convert to UTC and add one day to include the entire end date
            var endUtc = DateTime.SpecifyKind(endDate.Value.Date, DateTimeKind.Utc).AddDays(1);
            query = query.Where(a => a.Timestamp < endUtc);
        }

        if (memberId.HasValue)
        {
            query = query.Where(a => a.MemberId == memberId.Value);
        }

        if (!string.IsNullOrWhiteSpace(action))
        {
            query = query.Where(a => a.Action == action);
        }

        if (!string.IsNullOrWhiteSpace(actor))
        {
            query = query.Where(a => a.Actor.Contains(actor));
        }

        query = query.OrderByDescending(a => a.Timestamp);

        var auditLogs = await query.ToListAsync();
        return Ok(auditLogs);
    }
}
