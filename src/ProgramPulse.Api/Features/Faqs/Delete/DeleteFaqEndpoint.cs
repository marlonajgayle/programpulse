using ProgramPulse.Api.Domain.Authorization;
using ProgramPulse.Api.SharedKernel;
using ProgramPulse.Api.SharedKernel.Primitives;
using ProgramPulse.Api.SharedKernel.Versioning;

namespace ProgramPulse.Api.Features.Faqs.Delete;

/// <summary>
/// Soft-deletes a FAQ. Restricted to administrators.
/// </summary>
public sealed class DeleteFaqEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("faqs/{id:guid}", async (
            Guid id,
            DeleteFaqCommandHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(new DeleteFaqCommand(id), cancellationToken);
            return result.ToHttpResult();
        })
        .HasApiVersion(ApiVersions.V1)
        .RequireAuthorization(AuthorizationPolicies.AdminOnly)
        .WithName("DeleteFaq")
        .WithTags("FAQs");
    }
}
