using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Components.WebAssembly.Http;

namespace ProgramPulse.Web.Services;

/// <summary>
/// Typed client for the API's authenticated Objectives write endpoints. Wraps
/// <see cref="HttpClient"/> and translates RFC-7807 <c>ProblemDetails</c> failures into an
/// <see cref="AuthResult"/> the dialogs can render directly. Objectives are read as part of
/// the programme detail (see <see cref="ProgrammesApiClient.GetProgrammeDetailAsync"/>).
/// </summary>
public sealed class ObjectivesApiClient(HttpClient httpClient)
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task<AuthResult> CreateObjectiveAsync(
        Guid programmeId, CreateObjectiveRequest request, CancellationToken cancellationToken)
    {
        using var message = new HttpRequestMessage(
            HttpMethod.Post, $"api/v1/programmes/{programmeId}/objectives")
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

    public async Task<AuthResult> UpdateObjectiveAsync(
        Guid objectiveId, UpdateObjectiveRequest request, CancellationToken cancellationToken)
    {
        using var message = new HttpRequestMessage(
            HttpMethod.Put, $"api/v1/objectives/{objectiveId}")
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

/// <summary>Body posted to <c>POST api/v1/programmes/{programmeId}/objectives</c>.</summary>
public sealed record CreateObjectiveRequest(string Name, string Description);

/// <summary>Body sent to <c>PUT api/v1/objectives/{id}</c>.</summary>
public sealed record UpdateObjectiveRequest(string Name, string Description);
