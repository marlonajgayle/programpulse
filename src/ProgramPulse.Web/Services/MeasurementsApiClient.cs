using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using ProgramPulse.Web.Models;

namespace ProgramPulse.Web.Services;

/// <summary>
/// Typed client for the API's authenticated measurement endpoints. Wraps <see cref="HttpClient"/>,
/// returns <c>null</c> from reads when the session is missing/expired or the server is unreachable,
/// and (for writes) translates RFC-7807 <c>ProblemDetails</c> failures into an
/// <see cref="AuthResult"/> the dialog can render directly. Measurements are owned by a KPI, so the
/// list/create endpoints are nested under <c>kpis/{kpiId}</c>, while update targets the flat
/// <c>measurements/{id}</c> route.
/// </summary>
public sealed class MeasurementsApiClient(HttpClient httpClient)
{
    private readonly HttpClient _httpClient = httpClient;

    /// <summary>
    /// Lists a KPI's measurements, or <c>null</c> when the session is missing/expired (401),
    /// the KPI isn't found (404), or the server is unreachable.
    /// </summary>
    public async Task<IReadOnlyList<MeasurementResponse>?> GetMeasurementsAsync(
        Guid kpiId, CancellationToken cancellationToken)
    {
        using var message = new HttpRequestMessage(
            HttpMethod.Get, $"api/v1/kpis/{kpiId}/measurements");

        // Include credentials so the browser sends the auth cookie the endpoint authenticates with.
        message.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

        try
        {
            using var response = await _httpClient.SendAsync(message, cancellationToken);

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<IReadOnlyList<MeasurementResponse>>(cancellationToken);
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

    public async Task<AuthResult> CreateMeasurementAsync(
        Guid kpiId, CreateMeasurementRequest request, CancellationToken cancellationToken)
    {
        using var message = new HttpRequestMessage(
            HttpMethod.Post, $"api/v1/kpis/{kpiId}/measurements")
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

    public async Task<AuthResult> UpdateMeasurementAsync(
        Guid measurementId, UpdateMeasurementRequest request, CancellationToken cancellationToken)
    {
        using var message = new HttpRequestMessage(
            HttpMethod.Put, $"api/v1/measurements/{measurementId}")
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

/// <summary>Body posted to <c>POST api/v1/kpis/{kpiId}/measurements</c>. The KPI id travels in the
/// route, so it is not part of the body.</summary>
public sealed record CreateMeasurementRequest(decimal Value, string? Notes);

/// <summary>Body sent to <c>PUT api/v1/measurements/{id}</c>. The measurement id travels in the
/// route, so it is not part of the body.</summary>
public sealed record UpdateMeasurementRequest(decimal Value, string? Notes);

/// <summary>A measurement as returned by <c>GET api/v1/kpis/{kpiId}/measurements</c>.</summary>
public sealed record MeasurementResponse(
    Guid Id,
    decimal Value,
    string? Notes,
    Guid KpiId,
    DateTime CreatedDate,
    DateTime? LastModifiedDate);
