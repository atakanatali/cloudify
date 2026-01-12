namespace Cloudify.Application.Dtos;

/// <summary>
/// Represents a port allocation outcome.
/// </summary>
public sealed class PortAllocationResultDto
{
    /// <summary>
    /// Gets or sets the allocated port.
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the requested port was honored.
    /// </summary>
    public bool IsRequestedPort { get; set; }
}
