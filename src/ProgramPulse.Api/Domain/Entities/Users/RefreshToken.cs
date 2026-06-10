namespace ProgramPulse.Api.Domain.Entities.Users;

/// <summary>
/// A persisted JWT refresh token. Tracks its own lifecycle (expiry/revocation)
/// and the user it was issued to. Intentionally a plain POCO — it does not use
/// the soft-delete/audit base classes, since revocation and expiry already model
/// its lifecycle.
/// </summary>
public class RefreshToken
{
    public int Id { get; set; }
    public string JwtId { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RevokedAt { get; set; }
    public string? ReplacedByToken { get; set; }
    public string? ReasonRevoked { get; set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsRevoked => RevokedAt != null;
    public bool IsActive => !IsRevoked && !IsExpired;

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    public static RefreshToken Create(
        string jwtId,
        string token,
        DateTime expiresAt,
        string userId)
    {
        return new RefreshToken
        {
            JwtId = jwtId,
            Token = token,
            ExpiresAt = expiresAt,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Revoke(
        string? reason = null,
        string? replacedByToken = null)
    {
        if (IsRevoked)
            return;

        RevokedAt = DateTime.UtcNow;
        ReasonRevoked = reason;
        ReplacedByToken = replacedByToken;
    }
}
