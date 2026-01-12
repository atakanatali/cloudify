namespace Cloudify.Infrastructure.Persistence.Entities;

/// <summary>
/// Represents a persisted credential profile record.
/// </summary>
public sealed class CredentialProfileRecord
{
    /// <summary>
    /// Gets or sets the resource identifier for the credential profile.
    /// </summary>
    public Guid ResourceId { get; set; }

    /// <summary>
    /// Gets or sets the credential username.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the credential password.
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the resource navigation for the credentials.
    /// </summary>
    public ResourceRecord? Resource { get; set; }
}
