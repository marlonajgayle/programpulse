using ProgramPulse.Api.SharedKernel.Primitives;

namespace ProgramPulse.Api.Domain.Entities.Tenants.Programmes;

public static class MeasurementCommentErrors
{
    public static Error CommentNotFound(Guid commentId) => Error.NotFound(
        code: "MeasurementComment.NotFound",
        message: $"Measurement comment with ID '{commentId}' was not found.");
}
