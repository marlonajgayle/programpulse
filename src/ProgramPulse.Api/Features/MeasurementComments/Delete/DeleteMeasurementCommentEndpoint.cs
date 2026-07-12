using ProgramPulse.Api.Domain.Authorization;
using ProgramPulse.Api.SharedKernel;
using ProgramPulse.Api.SharedKernel.Primitives;
using ProgramPulse.Api.SharedKernel.Versioning;

namespace ProgramPulse.Api.Features.MeasurementComments.Delete;

/// <summary>
/// Soft-deletes a measurement comment within the caller's tenant.
/// </summary>
public sealed class DeleteMeasurementCommentEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("comments/{id:guid}", async (
            Guid id,
            DeleteMeasurementCommentCommandHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(new DeleteMeasurementCommentCommand(id), cancellationToken);
            return result.ToHttpResult();
        })
        .HasApiVersion(ApiVersions.V1)
        .RequireAuthorization(AuthorizationPolicies.Authenticated)
        .WithName("DeleteMeasurementComment")
        .WithTags("MeasurementComments");
    }
}
