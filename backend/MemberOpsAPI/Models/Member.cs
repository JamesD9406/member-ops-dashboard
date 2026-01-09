namespace MemberOpsAPI.Models;

public class Member
{
    public int Id { get; set; }
    public required string MemberNumber { get; set; } 
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required string Phone { get; set; }
    public required string Status { get; set; }
    public DateTime JoinDate { get; set; }
    public string? Notes { get; set; }
    
    // Navigation properties
    public ICollection<AccountFlag> Flags { get; set; } = new List<AccountFlag>();
    public ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}
