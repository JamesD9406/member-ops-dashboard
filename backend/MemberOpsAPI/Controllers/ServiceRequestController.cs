using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MemberOpsAPI.Data;
using MemberOpsAPI.Models;
using MemberOpsAPI.Constants;

namespace MemberOpsAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/service-requests")]
public class ServiceRequestsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ServiceRequestsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ServiceRequest>>> GetServiceRequests(
        [FromQuery] ServiceRequestStatus? status,
        [FromQuery] ServiceRequestPriority? priority,
        [FromQuery] int? assignedToId,
        [FromQuery] int? memberId)
    {
        var query = _context.ServiceRequests
            .Include(sr => sr.Member)
            .Include(sr => sr.CreatedBy)
            .Include(sr => sr.AssignedTo)
            .Include(sr => sr.ResolvedBy)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(sr => sr.Status == status.Value);
        }

        if (priority.HasValue)
        {
            query = query.Where(sr => sr.Priority == priority.Value);
        }

        if (assignedToId.HasValue)
        {
            query = query.Where(sr => sr.AssignedToId == assignedToId.Value);
        }

        if (memberId.HasValue)
        {
            query = query.Where(sr => sr.MemberId == memberId.Value);
        }

        query = query.OrderByDescending(sr => sr.CreatedAt);

        var serviceRequests = await query.ToListAsync();
        return Ok(serviceRequests);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ServiceRequest>> GetServiceRequest(int id)
    {
        var serviceRequest = await _context.ServiceRequests
            .Include(sr => sr.Member)
            .Include(sr => sr.CreatedBy)
            .Include(sr => sr.AssignedTo)
            .Include(sr => sr.ResolvedBy)
            .Include(sr => sr.Comments)
                .ThenInclude(c => c.Staff)
            .FirstOrDefaultAsync(sr => sr.Id == id);

        if (serviceRequest == null)
        {
            return NotFound(new { message = "Service request not found" });
        }

        return Ok(serviceRequest);
    }

    [HttpPost]
    public async Task<ActionResult<ServiceRequest>> CreateServiceRequest(
        [FromBody] CreateServiceRequestRequest request)
    {
        var member = await _context.Members.FindAsync(request.MemberId);
        if (member == null)
        {
            return NotFound(new { message = "Member not found" });
        }

        var username = User.Identity?.Name ?? "Unknown";
        var staff = await _context.Staff.FirstOrDefaultAsync(s => s.Username == username);

        if (staff == null)
        {
            return Unauthorized(new { message = "Staff not found" });
        }

        var serviceRequest = new ServiceRequest
        {
            MemberId = request.MemberId,
            RequestType = request.RequestType,
            Description = request.Description,
            Priority = request.Priority,
            Status = ServiceRequestStatus.New,
            CreatedById = staff.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.ServiceRequests.Add(serviceRequest);
        await _context.SaveChangesAsync();

        // Log the creation
        await _context.LogAuditAsync(
            request.MemberId,
            username,
            ServiceRequestActions.RequestCreated,
            $"Type: {request.RequestType}, Priority: {request.Priority} - {request.Description}"
        );

        return CreatedAtAction(nameof(GetServiceRequest), new { id = serviceRequest.Id }, serviceRequest);
    }

    [Authorize(Roles = "Supervisor,Admin")]
    [HttpPut("{id}/assign")]
    public async Task<ActionResult<ServiceRequest>> AssignServiceRequest(
        int id,
        [FromBody] AssignServiceRequestRequest request)
    {
        var serviceRequest = await _context.ServiceRequests
            .Include(sr => sr.Member)
            .Include(sr => sr.AssignedTo)
            .FirstOrDefaultAsync(sr => sr.Id == id);

        if (serviceRequest == null)
        {
            return NotFound(new { message = "Service request not found" });
        }

        var assignedToStaff = await _context.Staff.FindAsync(request.AssignedToId);
        if (assignedToStaff == null)
        {
            return NotFound(new { message = "Staff member not found" });
        }

        var username = User.Identity?.Name ?? "Unknown";

        serviceRequest.AssignedToId = request.AssignedToId;
        serviceRequest.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Log the assignment
        await _context.LogAuditAsync(
            serviceRequest.MemberId,
            username,
            ServiceRequestActions.RequestAssigned,
            $"Service Request #{id} assigned to {assignedToStaff.DisplayName}"
        );

        return Ok(serviceRequest);
    }

    [HttpPut("{id}/status")]
    public async Task<ActionResult<ServiceRequest>> UpdateServiceRequestStatus(
        int id,
        [FromBody] UpdateStatusRequest request)
    {
        var serviceRequest = await _context.ServiceRequests
            .Include(sr => sr.Member)
            .FirstOrDefaultAsync(sr => sr.Id == id);

        if (serviceRequest == null)
        {
            return NotFound(new { message = "Service request not found" });
        }

        var username = User.Identity?.Name ?? "Unknown";
        var oldStatus = serviceRequest.Status;

        serviceRequest.Status = request.Status;
        serviceRequest.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Log the status change
        await _context.LogAuditAsync(
            serviceRequest.MemberId,
            username,
            ServiceRequestActions.RequestStatusChanged,
            $"Service Request #{id} status changed from {oldStatus} to {request.Status}"
        );

        return Ok(serviceRequest);
    }

    [HttpPut("{id}/resolve")]
    public async Task<ActionResult<ServiceRequest>> ResolveServiceRequest(
        int id,
        [FromBody] ResolveServiceRequestRequest request)
    {
        var serviceRequest = await _context.ServiceRequests
            .Include(sr => sr.Member)
            .FirstOrDefaultAsync(sr => sr.Id == id);

        if (serviceRequest == null)
        {
            return NotFound(new { message = "Service request not found" });
        }

        if (serviceRequest.Status == ServiceRequestStatus.Resolved)
        {
            return BadRequest(new { message = "Service request is already resolved" });
        }

        var username = User.Identity?.Name ?? "Unknown";
        var staff = await _context.Staff.FirstOrDefaultAsync(s => s.Username == username);

        if (staff == null)
        {
            return Unauthorized(new { message = "Staff not found" });
        }

        serviceRequest.Status = ServiceRequestStatus.Resolved;
        serviceRequest.ResolutionType = request.ResolutionType;
        serviceRequest.ResolutionNotes = request.ResolutionNotes;
        serviceRequest.ResolvedAt = DateTime.UtcNow;
        serviceRequest.ResolvedById = staff.Id;
        serviceRequest.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Log the resolution
        await _context.LogAuditAsync(
            serviceRequest.MemberId,
            username,
            ServiceRequestActions.RequestResolved,
            $"Service Request #{id} resolved as {request.ResolutionType}" +
            (string.IsNullOrWhiteSpace(request.ResolutionNotes) ? "" : $" - Notes: {request.ResolutionNotes}")
        );

        return Ok(serviceRequest);
    }

    [HttpPost("{id}/comments")]
    public async Task<ActionResult<ServiceRequestComment>> AddComment(
        int id,
        [FromBody] AddCommentRequest request)
    {
        var serviceRequest = await _context.ServiceRequests
            .Include(sr => sr.Member)
            .FirstOrDefaultAsync(sr => sr.Id == id);

        if (serviceRequest == null)
        {
            return NotFound(new { message = "Service request not found" });
        }

        var username = User.Identity?.Name ?? "Unknown";
        var staff = await _context.Staff.FirstOrDefaultAsync(s => s.Username == username);

        if (staff == null)
        {
            return Unauthorized(new { message = "Staff not found" });
        }

        var comment = new ServiceRequestComment
        {
            ServiceRequestId = id,
            StaffId = staff.Id,
            CommentText = request.CommentText,
            CreatedAt = DateTime.UtcNow
        };

        _context.ServiceRequestComments.Add(comment);

        serviceRequest.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Log the comment addition
        await _context.LogAuditAsync(
            serviceRequest.MemberId,
            username,
            ServiceRequestActions.CommentAdded,
            $"Comment added to Service Request #{id}"
        );

        // Load the staff relationship for the response
        await _context.Entry(comment).Reference(c => c.Staff).LoadAsync();

        return Ok(comment);
    }
}

// DTOs for request bodies
public class CreateServiceRequestRequest
{
    public int MemberId { get; set; }
    public required string RequestType { get; set; }
    public required string Description { get; set; }
    public ServiceRequestPriority Priority { get; set; } = ServiceRequestPriority.Medium;
}

public class AssignServiceRequestRequest
{
    public int AssignedToId { get; set; }
}

public class UpdateStatusRequest
{
    public ServiceRequestStatus Status { get; set; }
}

public class ResolveServiceRequestRequest
{
    public ResolutionType ResolutionType { get; set; }
    public string? ResolutionNotes { get; set; }
}

public class AddCommentRequest
{
    public required string CommentText { get; set; }
}
