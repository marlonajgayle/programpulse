using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using ProgramPulse.Web.Models;

namespace ProgramPulse.Web.Services;

/// <summary>
/// Typed client for the API's authenticated Programmes endpoints. Wraps <see cref="HttpClient"/>,
/// returns <c>null</c> from reads when the session is missing/expired or the server is unreachable,
/// and (for writes) translates RFC-7807 <c>ProblemDetails</c> failures into an
/// <see cref="AuthResult"/> the dialog can render directly.
/// </summary>
public sealed class ProgrammesApiClient(HttpClient httpClient)
{
    private readonly HttpClient _httpClient = httpClient;

    /// <summary>
    /// Lists a page of the tenant's top-level programmes (each with its sub-programmes),
    /// or <c>null</c> when the session is missing/expired (401) or the server is unreachable.
    /// </summary>
    public async Task<PagedResponse<ProgrammeResponse>?> GetProgrammesAsync(
        int page, int pageSize, CancellationToken cancellationToken)
    {
        using var message = new HttpRequestMessage(
            HttpMethod.Get, $"api/v1/programmes?page={page}&pageSize={pageSize}");

        // Include credentials so the browser sends the auth cookie the endpoint authenticates with.
        message.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

        try
        {
            using var response = await _httpClient.SendAsync(message, cancellationToken);

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<PagedResponse<ProgrammeResponse>>(cancellationToken);
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

    /// <summary>
    /// Fetches a single programme with its objectives and KPIs, or <c>null</c> when it
    /// isn't found (404), the session is missing/expired (401), or the server is unreachable.
    /// </summary>
    public async Task<ProgrammeDetailResponse?> GetProgrammeDetailAsync(
        Guid id, CancellationToken cancellationToken)
    {
        using var message = new HttpRequestMessage(HttpMethod.Get, $"api/v1/programmes/{id}");

        // Include credentials so the browser sends the auth cookie the endpoint authenticates with.
        message.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

        try
        {
            using var response = await _httpClient.SendAsync(message, cancellationToken);

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<ProgrammeDetailResponse>(cancellationToken);
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

    public async Task<AuthResult> CreateProgrammeAsync(
        CreateProgrammeRequest request, CancellationToken cancellationToken)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, "api/v1/programmes")
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

    public async Task<AuthResult> UpdateProgrammeAsync(
        Guid id, UpdateProgrammeRequest request, CancellationToken cancellationToken)
    {
        using var message = new HttpRequestMessage(HttpMethod.Put, $"api/v1/programmes/{id}")
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

/// <summary>A single page of results with paging metadata, mirroring the API's
/// <c>PagedList&lt;T&gt;</c>.</summary>
public sealed record PagedResponse<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);

/// <summary>A programme as returned by <c>GET api/v1/programmes</c>. Top-level programmes
/// carry their <see cref="SubProgrammes"/>; sub-programmes have <see cref="ParentProgrammeId"/> set.</summary>
public sealed record ProgrammeResponse(
    Guid Id,
    string Name,
    string Description,
    DateTime? StartDate,
    DateTime? EndDate,
    ProgrammeStatus Status,
    DateTime CreatedDate,
    DateTime? LastModifiedDate,
    int ObjectiveCount,
    int KpiCount,
    Guid? ParentProgrammeId = null,
    IReadOnlyList<ProgrammeResponse>? SubProgrammes = null);

/// <summary>Body posted to <c>POST api/v1/programmes</c>.</summary>
public sealed record CreateProgrammeRequest(
    string Name,
    string Description,
    DateTime? StartDate,
    DateTime? EndDate,
    Guid? ParentProgrammeId = null);

/// <summary>Body sent to <c>PUT api/v1/programmes/{id}</c>.</summary>
public sealed record UpdateProgrammeRequest(
    string Name,
    string Description,
    DateTime? StartDate,
    DateTime? EndDate,
    Guid? ParentProgrammeId = null);

/// <summary>A single programme with its objectives → KPIs sub-tree, from
/// <c>GET api/v1/programmes/{id}</c>. Mirrors the API's nested response contract.</summary>
public sealed record ProgrammeDetailResponse(
    Guid Id,
    string Name,
    string Description,
    DateTime? StartDate,
    DateTime? EndDate,
    ProgrammeStatus Status,
    DateTime CreatedDate,
    DateTime? LastModifiedDate,
    IReadOnlyList<ObjectiveDetailResponse> Objectives);

/// <summary>An objective nested within an <see cref="ProgrammeDetailResponse"/>, with its KPIs.</summary>
public sealed record ObjectiveDetailResponse(
    Guid Id,
    string Name,
    string Description,
    Guid ProgrammeId,
    DateTime CreatedDate,
    DateTime? LastModifiedDate,
    IReadOnlyList<KpiDetailResponse> Kpis);

/// <summary>A KPI nested within an <see cref="ObjectiveDetailResponse"/>. Enum fields arrive
/// as numbers over the wire; ordinals match the Web <see cref="KpiDirection"/>/<see cref="KpiStatus"/>.</summary>
public sealed record KpiDetailResponse(
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
