namespace Cloudify.Application.Dtos;

/// <summary>
/// Represents credential settings supplied for a resource.
/// </summary>
public sealed class CredentialProfileDto
{
    /// <summary>
    /// Gets or sets the credential username.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the credential password.
    /// </summary>
    public string Password { get; set; } = string.Empty;
}
