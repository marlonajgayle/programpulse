using ProgramPulse.Api.Domain.Authorization;
using ProgramPulse.Api.SharedKernel;
using ProgramPulse.Api.SharedKernel.Primitives;
using ProgramPulse.Api.SharedKernel.Validation;
using ProgramPulse.Api.SharedKernel.Versioning;

namespace ProgramPulse.Api.Features.Faqs.Create;


/// <summary>
/// Creates a new FAQ. Restricted to administrators.
/// </summary>
public sealed class CreateFaqEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("faqs", async (
            CreateFaqCommand command,
            CreateFaqCommandHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(command, cancellationToken);
            return result.ToHttpResult();
        })
        .HasApiVersion(ApiVersions.V1)
        .WithValidation<CreateFaqCommand>()
        .RequireAuthorization(AuthorizationPolicies.AdminOnly)
        .WithName("CreateFaq")
        .WithTags("FAQs");
    }
}
