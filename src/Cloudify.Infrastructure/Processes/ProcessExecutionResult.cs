namespace Cloudify.Infrastructure.Processes;

/// <summary>
/// Represents the outcome of a process execution.
/// </summary>
public sealed class ProcessExecutionResult
{
    /// <summary>
    /// Gets or sets the exit code.
    /// </summary>
    public int? ExitCode { get; set; }

    /// <summary>
    /// Gets or sets the process standard output.
    /// </summary>
    public string StandardOutput { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the process standard error.
    /// </summary>
    public string StandardError { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the error code.
    /// </summary>
    public ProcessErrorCode ErrorCode { get; set; }

    /// <summary>
    /// Gets a value indicating whether the process succeeded.
    /// </summary>
    public bool IsSuccess => ErrorCode == ProcessErrorCode.None;

    /// <summary>
    /// Throws a <see cref="ProcessExecutionException"/> when the execution failed.
    /// </summary>
    /// <param name="command">The command executed.</param>
    /// <param name="arguments">The arguments passed.</param>
    public void EnsureSuccess(string command, IReadOnlyList<string> arguments)
    {
        if (IsSuccess)
        {
            return;
        }

        if (ErrorCode == ProcessErrorCode.Cancelled)
        {
            throw new OperationCanceledException($"Process '{command}' was cancelled.");
        }

        if (ErrorCode == ProcessErrorCode.Timeout)
        {
            throw new TimeoutException($"Process '{command}' exceeded the configured timeout.");
        }

        throw new ProcessExecutionException(command, arguments, this);
    }
}
