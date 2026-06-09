using Asp.Versioning;
using ProgramPulse.Api.SharedKernel;
using ProgramPulse.Api.SharedKernel.Versioning;

namespace ProgramPulse.Api.Features.Ping;

public sealed class PingEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("ping", () => Results.Ok(new PingResponse("pong", "v1")))
           .HasApiVersion(ApiVersions.V1) // tells the API explorer which doc this lands in
           .WithName("Ping")
           .WithTags("Ping");
    }
}

public sealed record PingResponse(string Message, string Version);
