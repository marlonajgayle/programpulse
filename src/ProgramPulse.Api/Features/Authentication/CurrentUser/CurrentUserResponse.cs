namespace ProgramPulse.Api.Features.Authentication.CurrentUser;

public sealed record CurrentUserResponse(string? FirstName, string? LastName, string? Email);
