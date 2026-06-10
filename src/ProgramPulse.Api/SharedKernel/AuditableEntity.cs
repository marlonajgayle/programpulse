namespace ProgramPulse.Api.SharedKernel;

public abstract class AuditableEntity<T> : AggregateRoot<T>, IAuditableEntity
{
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTime CreatedDate { get;  private set; }
    public string? LastModifiedBy { get; private set; }
    public DateTime? LastModifiedDate { get; private set; }

    public void SetCreatedAuditInfo(string createdBy)
    {
        CreatedBy = createdBy;
        CreatedDate = DateTime.UtcNow;
    }
    public void SetModifiedAuditInfo(string modifiedBy)
    {
        LastModifiedBy = modifiedBy;
        LastModifiedDate = DateTime.UtcNow;
    }
}