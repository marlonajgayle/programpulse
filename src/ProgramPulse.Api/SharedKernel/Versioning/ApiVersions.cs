using Asp.Versioning;

namespace ProgramPulse.Api.SharedKernel.Versioning;

public static class ApiVersions
{
    public static readonly ApiVersion V1 = new(1, 0);

    // Every supported version. Drives OpenAPI document registration AND the version set.
    public static readonly IReadOnlyList<ApiVersion> All = [V1];
}
