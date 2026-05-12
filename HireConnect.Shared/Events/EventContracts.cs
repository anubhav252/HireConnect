namespace HireConnect.Shared.Events
{
    public record JobPostedEvent(
        int JobId,
        int RecruiterId,
        string Title,
        string CompanyName,
        string Location,
        string Category,
        DateTime PostedAt
    );

    public record ApplicationSubmittedEvent(
        int ApplicationId,
        int JobId,
        int RecruiterId,
        int CandidateId,
        string CandidateEmail,
        string JobTitle,
        DateTime AppliedAt
    );

    public record ApplicationStatusChangedEvent(
        int ApplicationId,
        int RecruiterId,
        int? CandidateId,
        string NewStatus,
        string OldStatus,
        string CandidateEmail,
        string JobTitle,
        string RecruiterMessage
    );

    public record InterviewScheduledEvent(
        int InterviewId,
        int ApplicationId,
        int? CandidateId,
        string CandidateEmail,
        DateTime ScheduledAt,
        string Mode,
        string MeetLink,
        string Location
    );

    public record SubscriptionCreatedEvent(
        int SubscriptionId,
        int RecruiterId,
        string Plan,
        DateTime EndDate
    );
    public record UserRegisteredEvent(int UserId, string Email, string FullName, string Role);
}
