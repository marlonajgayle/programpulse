namespace ProgramPulse.Api.SharedKernel;

public abstract class BaseEntity<TId>
{
    public  TId? Id { get; init; }

    // Soft delete
    public bool IsDeleted { get; protected set; }
    public DateTime? DeletedUtc { get; protected set; }

    protected BaseEntity() {}

    protected BaseEntity(TId id)
    {
        Id = id;
    }

    public void MarkAsDeleted()
    {
        IsDeleted = true;
        DeletedUtc = DateTime.UtcNow;
    }

    public void Restore()
    {
        IsDeleted = false;
        DeletedUtc = null;
    }
}