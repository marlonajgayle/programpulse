namespace ProgramPulse.Api.SharedKernel;

/// <summary>
/// Marker for entities that are soft-deleted rather than physically removed.
/// Lets the persistence layer reach soft-delete behaviour without knowing the
/// concrete <see cref="BaseEntity{TId}"/> key type.
/// </summary>
public interface ISoftDeletable
{
    bool IsDeleted { get; }

    void MarkAsDeleted();
}
