namespace MemberOpsAPI.Models;

public class Staff
{
    public int Id { get; set; }
    public required string Username { get; set; }
    public required string PasswordHash { get; set; }
    public required string DisplayName { get; set; }
    public required string Email { get; set; }
    public required string Role { get; set; } // "Staff", "Supervisor", "Admin"
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
