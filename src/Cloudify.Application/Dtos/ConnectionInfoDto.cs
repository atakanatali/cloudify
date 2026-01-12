namespace Cloudify.Application.Dtos;

/// <summary>
/// Represents a computed connection endpoint for a resource.
/// </summary>
public sealed class ConnectionInfoDto
{
    /// <summary>
    /// Gets or sets the connection host name.
    /// </summary>
    public string Host { get; set; } = "localhost";

    /// <summary>
    /// Gets or sets the connection port number.
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// Gets or sets the username for the connection when authentication is required.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Gets or sets the password for the connection when authentication is required.
    /// </summary>
    public string? Password { get; set; }
}
