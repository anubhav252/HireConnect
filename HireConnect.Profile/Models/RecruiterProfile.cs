namespace HireConnect.Profile.Models;

public class RecruiterProfile
{
    public int      ProfileId   { get; set; }

    // Matches UserId from auth-service
    public int      UserId      { get; set; }

    public string   FullName    { get; set; } = string.Empty;
    public string   Email       { get; set; } = string.Empty;
    public string   Mobile      { get; set; } = string.Empty;
    public string   CompanyName { get; set; } = string.Empty;
    public string   CompanySize { get; set; } = string.Empty;   // e.g. "1-10", "11-50", "51-200"
    public string   Industry    { get; set; } = string.Empty;
    public string   Website     { get; set; } = string.Empty;
    public string   Bio         { get; set; } = string.Empty;
    public string   LogoUrl     { get; set; } = string.Empty;

    public DateTime CreatedAt   { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt   { get; set; } = DateTime.UtcNow;

    // Navigation
    public List<Address> Addresses { get; set; } = [];

    public int    GetProfileId()              => ProfileId;
    public string GetCompanyName()            => CompanyName;
    public void   SetCompanyName(string name) => CompanyName = name;
    public override string ToString()         => $"{FullName} @ {CompanyName}";
}
