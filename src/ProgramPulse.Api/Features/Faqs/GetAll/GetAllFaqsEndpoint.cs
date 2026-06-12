using ProgramPulse.Api.SharedKernel;
using ProgramPulse.Api.SharedKernel.Primitives;
using ProgramPulse.Api.SharedKernel.Versioning;

namespace ProgramPulse.Api.Features.Faqs.GetAll;

/// <summary>
/// Returns all FAQs. Publicly accessible.
/// </summary>
public sealed class GetAllFaqsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("faqs", async (
            GetAllFaqsQueryHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(new GetAllFaqsQuery(), cancellationToken);
            return result.ToHttpResult();
        })
        .HasApiVersion(ApiVersions.V1)
        .WithName("GetAllFaqs")
        .WithTags("FAQs");
    }
}
