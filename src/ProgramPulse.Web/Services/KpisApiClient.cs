using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using ProgramPulse.Web.Models;

namespace ProgramPulse.Web.Services;

/// <summary>
/// Typed client for the API's authenticated KPI endpoints. Wraps <see cref="HttpClient"/>,
/// returns <c>null</c> from reads when the session is missing/expired or the server is unreachable,
/// and (for writes) translates RFC-7807 <c>ProblemDetails</c> failures into an
/// <see cref="AuthResult"/> the dialog can render directly. KPIs are owned by an objective, so
/// both endpoints are nested under <c>objectives/{objectiveId}</c>.
/// </summary>
public sealed class KpisApiClient(HttpClient httpClient)
{
    private readonly HttpClient _httpClient = httpClient;

    /// <summary>
    /// Lists an objective's KPIs, or <c>null</c> when the session is missing/expired (401),
    /// the objective isn't found (404), or the server is unreachable.
    /// </summary>
    public async Task<IReadOnlyList<KpiResponse>?> GetKpisAsync(
        Guid objectiveId, CancellationToken cancellationToken)
    {
        using var message = new HttpRequestMessage(
            HttpMethod.Get, $"api/v1/objectives/{objectiveId}/kpis");

        // Include credentials so the browser sends the auth cookie the endpoint authenticates with.
        message.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

        try
        {
            using var response = await _httpClient.SendAsync(message, cancellationToken);

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<IReadOnlyList<KpiResponse>>(cancellationToken);
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

    public async Task<AuthResult> CreateKpiAsync(
        Guid objectiveId, CreateKpiRequest request, CancellationToken cancellationToken)
    {
        using var message = new HttpRequestMessage(
            HttpMethod.Post, $"api/v1/objectives/{objectiveId}/kpis")
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

/// <summary>Body posted to <c>POST api/v1/objectives/{objectiveId}/kpis</c>. The objective id
/// travels in the route, so it is not part of the body.</summary>
public sealed record CreateKpiRequest(
    string Name,
    string Unit,
    KpiDirection Direction,
    decimal BaselineValue,
    decimal TargetValue,
    decimal CurrentValue,
    DateTime DueDate);

/// <summary>A KPI as returned by <c>GET api/v1/objectives/{objectiveId}/kpis</c>. Enum fields
/// arrive as numbers over the wire; ordinals match the Web
/// <see cref="KpiDirection"/>/<see cref="KpiStatus"/>.</summary>
public sealed record KpiResponse(
    Guid Id,
    string Name,
    string Unit,
    KpiDirection Direction,
    decimal BaselineValue,
    decimal TargetValue,
    decimal CurrentValue,
    DateTime DueDate,
    KpiStatus Status,
    Guid ObjectiveId,
    DateTime CreatedDate,
    DateTime? LastModifiedDate);
