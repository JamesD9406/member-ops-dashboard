namespace MemberOpsAPI.Models;

public class AuditLog
{
    public int Id { get; set; }
    public int MemberId { get; set; }
    public required string Actor { get; set; } 
    public required string Action { get; set; } 
    public string? Details { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    // Navigation property
    public Member Member { get; set; } = null!;
}
