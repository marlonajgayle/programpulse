using ProgramPulse.Api.Domain.Entities.Users;
using ProgramPulse.Api.Infrastructure.Authentication;
using ProgramPulse.Api.SharedKernel.Primitives;

namespace ProgramPulse.Api.Features.Authentication.CurrentUser;

public sealed record GetCurrentUserQuery();

/// <summary>
/// Returns the authenticated caller's profile (name and email) projected straight from the
/// request's JWT claims — no database round-trip. The <c>Authenticated</c> policy on the
/// endpoint guarantees a principal; the null check is defensive.
/// </summary>
public sealed class GetCurrentUserQueryHandler(ICurrentUser currentUser)
{
    private readonly ICurrentUser _currentUser = currentUser;

    public Task<Result<CurrentUserResponse>> HandleAsync(
        GetCurrentUserQuery query,
        CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null)
        {
            return Task.FromResult(
                Result<CurrentUserResponse>.Failure(AuthenticationErrors.UserNotAuthenticated()));
        }

        var response = new CurrentUserResponse(
            _currentUser.FirstName,
            _currentUser.LastName,
            _currentUser.Email);

        return Task.FromResult(Result<CurrentUserResponse>.Success(response));
    }
}
