namespace HireConnect.Profile.Models;

public class Address
{
    public int AddressId { get; set; }
    public string HouseNo { get; set; } = string.Empty;
    public string Street  { get; set; } = string.Empty;
    public string City    { get; set; } = string.Empty;
    public string State   { get; set; } = string.Empty;
    public string Pincode { get; set; } = string.Empty;

    // FK back to profile
    public int    ProfileId   { get; set; }
    public string ProfileType { get; set; } = string.Empty;   // "Candidate" | "Recruiter"

    public string GetCity()  => City;
    public string GetState() => State;
    public void   SetPincode(string pincode) => Pincode = pincode;
    public override string ToString() => $"{HouseNo}, {Street}, {City}, {State} - {Pincode}";
}
