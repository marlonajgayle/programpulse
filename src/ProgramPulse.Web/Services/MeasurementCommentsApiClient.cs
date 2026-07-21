using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using ProgramPulse.Web.Models;

namespace ProgramPulse.Web.Services;

/// <summary>
/// Typed client for the API's authenticated measurement-comment endpoints. Wraps
/// <see cref="HttpClient"/>, returns <c>null</c> from the read when the session is
/// missing/expired or the server is unreachable, and (for the write) translates RFC-7807
/// <c>ProblemDetails</c> failures into an <see cref="AuthResult"/> the dialog can render
/// directly. Comments are owned by a Measurement, so the list/create endpoints are nested
/// under <c>measurements/{measurementId}</c>.
/// </summary>
public sealed class MeasurementCommentsApiClient(HttpClient httpClient)
{
    private readonly HttpClient _httpClient = httpClient;

    /// <summary>
    /// Lists a measurement's comments (oldest first), or <c>null</c> when the session is
    /// missing/expired (401), the measurement isn't found (404), or the server is unreachable.
    /// </summary>
    public async Task<IReadOnlyList<MeasurementCommentResponse>?> GetCommentsAsync(
        Guid measurementId, CancellationToken cancellationToken)
    {
        using var message = new HttpRequestMessage(
            HttpMethod.Get, $"api/v1/measurements/{measurementId}/comments");

        // Include credentials so the browser sends the auth cookie the endpoint authenticates with.
        message.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

        try
        {
            using var response = await _httpClient.SendAsync(message, cancellationToken);

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<IReadOnlyList<MeasurementCommentResponse>>(cancellationToken);
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

    public async Task<AuthResult> AddCommentAsync(
        Guid measurementId, CreateCommentRequest request, CancellationToken cancellationToken)
    {
        using var message = new HttpRequestMessage(
            HttpMethod.Post, $"api/v1/measurements/{measurementId}/comments")
        {
            Content = JsonContent.Create(request)
        };

        // The endpoint requires an authenticated user; include the auth cookie.
        message.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

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

/// <summary>Body posted to <c>POST api/v1/measurements/{measurementId}/comments</c>. The
/// measurement id travels in the route, so it is not part of the body.</summary>
public sealed record CreateCommentRequest(string Text);

/// <summary>A comment as returned by <c>GET api/v1/measurements/{measurementId}/comments</c>.</summary>
public sealed record MeasurementCommentResponse(
    Guid Id,
    string Text,
    Guid MeasurementId,
    string CreatedBy,
    DateTime CreatedDate,
    DateTime? LastModifiedDate);
