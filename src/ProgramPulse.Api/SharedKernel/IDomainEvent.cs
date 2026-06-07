namespace ProgramPulse.Api.SharedKernel;

public interface IDomainEvent
{
    DateTime OccurredOnUtc { get; }
}