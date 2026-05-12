namespace HireConnect.Profile.Models;

public class CandidateProfile
{
    public int      ProfileId  { get; set; }

    // Matches UserId from auth-service 
    public int      UserId     { get; set; }

    public string   FullName   { get; set; } = string.Empty;
    public string   Email      { get; set; } = string.Empty;
    public string   Mobile     { get; set; } = string.Empty;
    public DateTime Dob        { get; set; }
    public string   Bio        { get; set; } = string.Empty;

    // Stored as comma-separated; surfaced as List<string> via helper
    public string   SkillsRaw  { get; set; } = string.Empty;
    public int      Experience { get; set; }          // years
    public string   ResumeUrl  { get; set; } = string.Empty;

    public DateTime CreatedAt  { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt  { get; set; } = DateTime.UtcNow;

    // Navigation
    public List<Address> Addresses { get; set; } = [];

    // Helpers
    public List<string> GetSkills() =>
        string.IsNullOrWhiteSpace(SkillsRaw)
            ? []
            : [.. SkillsRaw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)];

    public void SetSkills(List<string> skills) =>
        SkillsRaw = string.Join(',', skills);

    public string GetEmail()    => Email;
    public override string ToString() => $"{FullName} [{Email}]";
}
