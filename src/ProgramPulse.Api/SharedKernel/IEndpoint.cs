namespace ProgramPulse.Api.SharedKernel;

public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}