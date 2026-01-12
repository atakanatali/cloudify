namespace Cloudify.Infrastructure.Processes;

/// <summary>
/// Represents a process execution request.
/// </summary>
public sealed class ProcessExecutionRequest
{
    /// <summary>
    /// Gets or sets the file name to execute.
    /// </summary>
    public required string FileName { get; set; }

    /// <summary>
    /// Gets or sets the process arguments.
    /// </summary>
    public IReadOnlyList<string> Arguments { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets the working directory.
    /// </summary>
    public string? WorkingDirectory { get; set; }

    /// <summary>
    /// Gets or sets the timeout for the process.
    /// </summary>
    public TimeSpan? Timeout { get; set; }
}
