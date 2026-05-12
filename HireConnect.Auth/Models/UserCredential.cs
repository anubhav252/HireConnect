namespace HireConnect.Auth.Models;

public class UserCredential
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;        // "Candidate" or "Recruiter"
    public string Provider { get; set; } = "Local";         // "Local" or "GitHub"
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string GetEmail() => Email;
    public string GetRole() => Role;
    public override string ToString() => $"{Email} [{Role}]";
}