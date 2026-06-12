using ProgramPulse.Api.Domain.Authorization;
using ProgramPulse.Api.SharedKernel;
using ProgramPulse.Api.SharedKernel.Primitives;
using ProgramPulse.Api.SharedKernel.Validation;
using ProgramPulse.Api.SharedKernel.Versioning;

namespace ProgramPulse.Api.Features.Faqs.Update;

/// <summary>
/// Updates an existing FAQ. The FAQ id comes from the route; the question and answer
/// come from the request body. Restricted to administrators.
/// </summary>
public sealed class UpdateFaqEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("faqs/{id:guid}", async (
            Guid id,
            UpdateFaqCommand command,
            UpdateFaqCommandHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(command with { Id = id }, cancellationToken);
            return result.ToHttpResult();
        })
        .HasApiVersion(ApiVersions.V1)
        .WithValidation<UpdateFaqCommand>()
        .RequireAuthorization(AuthorizationPolicies.AdminOnly)
        .WithName("UpdateFaq")
        .WithTags("FAQs");
    }
}
