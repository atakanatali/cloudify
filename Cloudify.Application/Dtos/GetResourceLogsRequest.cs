namespace Cloudify.Application.Dtos;

/// <summary>
/// Represents a request to retrieve resource logs.
/// </summary>
public sealed class GetResourceLogsRequest
{
    /// <summary>
    /// Gets or sets the resource identifier.
    /// </summary>
    public Guid ResourceId { get; set; }

    /// <summary>
    /// Gets or sets the number of log lines to tail.
    /// </summary>
    public int Tail { get; set; } = 100;

    /// <summary>
    /// Gets or sets the optional service name to target for logs.
    /// </summary>
    public string? ServiceName { get; set; }
}

/// <summary>
/// Represents the response for retrieving resource logs.
/// </summary>
public sealed class GetResourceLogsResponse
{
    /// <summary>
    /// Gets or sets the resource identifier.
    /// </summary>
    public Guid ResourceId { get; set; }

    /// <summary>
    /// Gets or sets the log output.
    /// </summary>
    public string Logs { get; set; } = string.Empty;
}
