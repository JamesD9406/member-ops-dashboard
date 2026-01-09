namespace MemberOpsAPI.Models;

public class ServiceRequest
{
    public int Id { get; set; }
    public int MemberId { get; set; }
    public required string RequestType { get; set; } 
    public required string Description { get; set; }
    public required string Status { get; set; }
    public required string CreatedBy { get; set; } 
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CompletedBy { get; set; }
    public DateTime? CompletedAt { get; set; }
    
    // Navigation property
    public Member Member { get; set; } = null!;
}
