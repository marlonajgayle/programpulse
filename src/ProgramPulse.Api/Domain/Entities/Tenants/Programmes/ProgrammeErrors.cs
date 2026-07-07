using ProgramPulse.Api.SharedKernel.Primitives;

namespace ProgramPulse.Api.Domain.Entities.Tenants.Programmes;

public static class ProgrammeErrors
{
    public static Error ProgrammeNotFound(Guid programmeId) => Error.NotFound(
        code: "Programme.NotFound",
        message: $"Programme with ID '{programmeId}' was not found.");

    public static Error ParentProgrammeNotFound(Guid parentProgrammeId) => Error.NotFound(
        code: "Programme.ParentNotFound",
        message: $"Parent programme with ID '{parentProgrammeId}' was not found.");

    public static readonly Error ParentIsSelf = Error.Validation(
        code: "Programme.ParentIsSelf",
        message: "A programme cannot be its own parent.");

    public static readonly Error ParentNotTopLevel = Error.Validation(
        code: "Programme.ParentNotTopLevel",
        message: "The chosen parent is itself a sub-programme; a programme can only nest one level deep.");

    public static readonly Error ProgrammeHasSubProgrammes = Error.Validation(
        code: "Programme.HasSubProgrammes",
        message: "This programme has sub-programmes, so it cannot become a sub-programme itself.");
}
