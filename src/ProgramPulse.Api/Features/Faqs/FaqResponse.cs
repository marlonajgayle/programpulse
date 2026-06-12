namespace ProgramPulse.Api.Features.Faqs;

/// <summary>
/// API representation of a FAQ returned by the read endpoints.
/// </summary>
public sealed record FaqResponse(
    Guid Id,
    string Question,
    string Answer,
    DateTime CreatedDate,
    DateTime? LastModifiedDate);
