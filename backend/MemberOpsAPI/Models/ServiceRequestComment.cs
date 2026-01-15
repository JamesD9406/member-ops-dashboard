namespace MemberOpsAPI.Models;

public class ServiceRequestComment
{
    public int Id { get; set; }
    public int ServiceRequestId { get; set; }
    public ServiceRequest ServiceRequest { get; set; } = null!;
    
    public int StaffId { get; set; }
    public Staff Staff { get; set; } = null!;
    
    public string CommentText { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
