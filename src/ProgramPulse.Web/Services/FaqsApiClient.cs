using System.Net.Http.Json;
using System.Text.Json;

namespace ProgramPulse.Web.Services;

/// <summary>
/// Typed client for the API's public FAQ endpoint. Wraps <see cref="HttpClient"/> and
/// returns <c>null</c> when the FAQs cannot be loaded, so callers can render an error state.
/// </summary>
public sealed class FaqsApiClient(HttpClient httpClient)
{
    private readonly HttpClient _httpClient = httpClient;

    /// <summary>Lists published FAQs, or <c>null</c> when the server is unreachable.</summary>
    public async Task<IReadOnlyList<FaqItem>?> GetFaqsAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var response = await _httpClient.GetAsync("api/v1/faqs", cancellationToken);

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<IReadOnlyList<FaqItem>>(cancellationToken);
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
}

/// <summary>An FAQ as returned by <c>GET api/v1/faqs</c>.</summary>
public sealed record FaqItem(Guid Id, string Question, string Answer);
