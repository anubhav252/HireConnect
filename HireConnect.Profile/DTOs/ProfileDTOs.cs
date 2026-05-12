namespace HireConnect.Profile.DTOs;

// ── Address ───────────────────────────────────────────────────────────────────

public record AddressDto(
    int    AddressId,
    string HouseNo,
    string Street,
    string City,
    string State,
    string Pincode
);

public record AddressRequest(
    string HouseNo,
    string Street,
    string City,
    string State,
    string Pincode
);

// ── Candidate ─────────────────────────────────────────────────────────────────

public record CreateCandidateProfileRequest(
    int              UserId,
    string           FullName,
    string           Email,
    string           Mobile,
    DateTime         Dob,
    string           Bio,
    List<string>     Skills,
    int              Experience,
    string           ResumeUrl,
    List<AddressRequest> Addresses
);

public record UpdateCandidateProfileRequest(
    string?          FullName,
    string?          Mobile,
    DateTime?        Dob,
    string?          Bio,
    List<string>?    Skills,
    int?             Experience,
    string?          ResumeUrl,
    List<AddressRequest>? Addresses
);

public record CandidateProfileResponse(
    int              ProfileId,
    int              UserId,
    string           FullName,
    string           Email,
    string           Mobile,
    DateTime         Dob,
    string           Bio,
    List<string>     Skills,
    int              Experience,
    string           ResumeUrl,
    DateTime         CreatedAt,
    DateTime         UpdatedAt,
    List<AddressDto> Addresses
);

// ── Recruiter ─────────────────────────────────────────────────────────────────

public record CreateRecruiterProfileRequest(
    int              UserId,
    string           FullName,
    string           Email,
    string           Mobile,
    string           CompanyName,
    string           CompanySize,
    string           Industry,
    string           Website,
    string           Bio,
    string           LogoUrl,
    List<AddressRequest> Addresses
);

public record UpdateRecruiterProfileRequest(
    string?          FullName,
    string?          Mobile,
    string?          CompanyName,
    string?          CompanySize,
    string?          Industry,
    string?          Website,
    string?          Bio,
    string?          LogoUrl,
    List<AddressRequest>? Addresses
);

public record RecruiterProfileResponse(
    int              ProfileId,
    int              UserId,
    string           FullName,
    string           Email,
    string           Mobile,
    string           CompanyName,
    string           CompanySize,
    string           Industry,
    string           Website,
    string           Bio,
    string           LogoUrl,
    DateTime         CreatedAt,
    DateTime         UpdatedAt,
    List<AddressDto> Addresses
);
