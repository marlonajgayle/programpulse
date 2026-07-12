using ProgramPulse.Api.Domain.Authorization;
using ProgramPulse.Api.SharedKernel;
using ProgramPulse.Api.SharedKernel.Primitives;
using ProgramPulse.Api.SharedKernel.Validation;
using ProgramPulse.Api.SharedKernel.Versioning;

namespace ProgramPulse.Api.Features.MeasurementComments.Update;

/// <summary>
/// Updates an existing measurement comment. The id comes from the route.
/// </summary>
public sealed class UpdateMeasurementCommentEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("comments/{id:guid}", async (
            Guid id,
            UpdateMeasurementCommentCommand command,
            UpdateMeasurementCommentCommandHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(command with { Id = id }, cancellationToken);
            return result.ToHttpResult();
        })
        .HasApiVersion(ApiVersions.V1)
        .WithValidation<UpdateMeasurementCommentCommand>()
        .RequireAuthorization(AuthorizationPolicies.Authenticated)
        .WithName("UpdateMeasurementComment")
        .WithTags("MeasurementComments");
    }
}
