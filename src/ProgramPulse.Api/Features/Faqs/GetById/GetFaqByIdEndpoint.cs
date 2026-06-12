using ProgramPulse.Api.SharedKernel;
using ProgramPulse.Api.SharedKernel.Primitives;
using ProgramPulse.Api.SharedKernel.Versioning;

namespace ProgramPulse.Api.Features.Faqs.GetById;

/// <summary>
/// Returns a single FAQ by id. Publicly accessible.
/// </summary>
public sealed class GetFaqByIdEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("faqs/{id:guid}", async (
            Guid id,
            GetFaqByIdQueryHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(new GetFaqByIdQuery(id), cancellationToken);
            return result.ToHttpResult();
        })
        .HasApiVersion(ApiVersions.V1)
        .WithName("GetFaqById")
        .WithTags("FAQs");
    }
}
