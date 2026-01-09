namespace MemberOpsAPI.Models;

public class AccountFlag
{
    public int Id { get; set; }
    public int MemberId { get; set; }
    public required string FlagType { get; set; } 
    public required string Description { get; set; }
    public required string CreatedBy { get; set; } 
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? ResolvedBy { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? ResolutionNotes { get; set; }
    
    // Navigation property
    public Member Member { get; set; } = null!;
}
