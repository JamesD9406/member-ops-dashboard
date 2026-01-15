namespace MemberOpsAPI.Models;

public class ServiceRequest
{
    public int Id { get; set; }
    public int MemberId { get; set; }
    public Member Member { get; set; } = null!;

    public string RequestType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ServiceRequestStatus Status { get; set; } = ServiceRequestStatus.New;
    public ServiceRequestPriority Priority { get; set; } = ServiceRequestPriority.Medium;

    public int CreatedById { get; set; }
    public Staff CreatedBy { get; set; } = null!;

    public int? AssignedToId { get; set; }
    public Staff? AssignedTo { get; set; }

    public ResolutionType? ResolutionType { get; set; }
    public string? ResolutionNotes { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public int? ResolvedById { get; set; }
    public Staff? ResolvedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ServiceRequestComment> Comments { get; set; } = new List<ServiceRequestComment>();
}

public enum ServiceRequestStatus
{
    New,
    InProgress,
    Resolved,
    Cancelled
}

public enum ServiceRequestPriority
{
    Low,
    Medium,
    High,
    Urgent
}

public enum ResolutionType
{
    Resolved,
    MoreInfoNeeded,
    Transferred,
    Duplicate,
    CannotResolve,
    Cancelled
}
