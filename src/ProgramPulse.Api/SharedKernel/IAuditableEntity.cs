namespace ProgramPulse.Api.SharedKernel;

/// <summary>
/// Marker for entities carrying audit metadata. Lets the persistence layer
/// stamp audit info without knowing the concrete <see cref="AuditableEntity{T}"/>
/// key type.
/// </summary>
public interface IAuditableEntity
{
    void SetCreatedAuditInfo(string createdBy);

    void SetModifiedAuditInfo(string modifiedBy);
}
