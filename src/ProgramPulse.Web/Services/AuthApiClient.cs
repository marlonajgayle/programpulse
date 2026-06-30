using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Components.WebAssembly.Http;

namespace ProgramPulse.Web.Services;

/// <summary>
/// Typed client for the API's authentication endpoints. Wraps <see cref="HttpClient"/> and
/// translates the API's RFC-7807 <c>ProblemDetails</c> failures into an <see cref="AuthResult"/>
/// the auth pages can render directly (a general message and/or per-field messages).
/// </summary>
public sealed class AuthApiClient(HttpClient httpClient)
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task<AuthResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, "api/v1/auth/register")
        {
            Content = JsonContent.Create(request)
        };

        return await SendAsync(message, cancellationToken);
    }

    public async Task<AuthResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, "api/v1/auth/login")
        {
            Content = JsonContent.Create(request)
        };

        // Login sets HttpOnly auth cookies on the response; include credentials so the
        // browser stores them and sends them on later API calls.
        message.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

        return await SendAsync(message, cancellationToken);
    }

    public async Task<AuthResult> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, "api/v1/auth/forgot-password")
        {
            Content = JsonContent.Create(request)
        };

        return await SendAsync(message, cancellationToken);
    }

    public async Task<AuthResult> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, "api/v1/auth/reset-password")
        {
            Content = JsonContent.Create(request)
        };

        return await SendAsync(message, cancellationToken);
    }

    public async Task<AuthResult> LogoutAsync(CancellationToken cancellationToken)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, "api/v1/auth/logout");

        // Include credentials so the browser sends the auth cookies (the endpoint needs them to
        // authenticate and to read the refresh token) and stores the cleared Set-Cookie response.
        message.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

        return await SendAsync(message, cancellationToken);
    }

    /// <summary>
    /// Fetches the signed-in user's profile, or <c>null</c> when the session is missing/expired
    /// (401) or the server is unreachable. The JWT lives in an HttpOnly cookie, so this is the
    /// only way the WASM client can learn who is logged in.
    /// </summary>
    public async Task<CurrentUserResponse?> GetCurrentUserAsync(CancellationToken cancellationToken)
    {
        using var message = new HttpRequestMessage(HttpMethod.Get, "api/v1/auth/me");

        // Include credentials so the browser sends the auth cookie the endpoint authenticates with.
        message.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

        try
        {
            using var response = await _httpClient.SendAsync(message, cancellationToken);

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<CurrentUserResponse>(cancellationToken);
        }
        catch (HttpRequestException)
        {
            return null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private async Task<AuthResult> SendAsync(HttpRequestMessage message, CancellationToken cancellationToken)
    {
        HttpResponseMessage response;
        try
        {
            response = await _httpClient.SendAsync(message, cancellationToken);
        }
        catch (HttpRequestException)
        {
            return AuthResult.Failure("Unable to reach the server. Please try again.");
        }

        if (response.IsSuccessStatusCode)
            return AuthResult.Ok();

        return await ParseProblemAsync(response, cancellationToken);
    }

    private static async Task<AuthResult> ParseProblemAsync(
        HttpResponseMessage response, CancellationToken cancellationToken)
    {
        ProblemResponse? problem = null;
        try
        {
            problem = await response.Content.ReadFromJsonAsync<ProblemResponse>(cancellationToken);
        }
        catch (JsonException)
        {
            // Body was not ProblemDetails JSON; fall through to a generic message.
        }

        if (problem?.Errors is { Count: > 0 } fieldErrors)
            return new AuthResult(false, problem.Detail ?? problem.Title, fieldErrors);

        var general = problem?.Detail ?? problem?.Title ?? "Something went wrong. Please try again.";
        return AuthResult.Failure(general);
    }
}

/// <summary>Outcome of an auth call: success, a general error, and/or per-field validation errors.</summary>
public sealed record AuthResult(
    bool Success,
    string? GeneralError,
    IReadOnlyDictionary<string, string[]>? FieldErrors)
{
    public static AuthResult Ok() => new(true, null, null);

    public static AuthResult Failure(string generalError) => new(false, generalError, null);
}

public sealed record RegisterRequest(
    string TenantName,
    string FirstName,
    string LastName,
    string Email,
    string Password);

public sealed record LoginRequest(string Email, string Password);

/// <summary>The signed-in user's profile, as returned by <c>GET auth/me</c>.</summary>
public sealed record CurrentUserResponse(string? FirstName, string? LastName, string? Email);

public sealed record ForgotPasswordRequest(string Email);

public sealed record ResetPasswordRequest(
    string Email,
    string Token,
    string NewPassword,
    string ConfirmPassword);

/// <summary>Subset of RFC-7807 ProblemDetails the API returns on failure.</summary>
internal sealed record ProblemResponse(
    string? Title,
    string? Detail,
    IReadOnlyDictionary<string, string[]>? Errors);
