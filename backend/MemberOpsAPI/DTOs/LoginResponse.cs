namespace MemberOpsAPI.DTOs;

public class LoginResponse
{
    public required string Token { get; set; }
    public required string Username { get; set; }
    public required string DisplayName { get; set; }
    public required string Role { get; set; }
    public DateTime ExpiresAt { get; set; }
}
