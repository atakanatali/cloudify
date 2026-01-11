namespace Cloudify.Infrastructure.Processes;

/// <summary>
/// Represents error categories for process execution.
/// </summary>
public enum ProcessErrorCode
{
    /// <summary>
    /// Indicates the process completed successfully.
    /// </summary>
    None = 0,

    /// <summary>
    /// Indicates the process failed to start.
    /// </summary>
    StartFailed = 1,

    /// <summary>
    /// Indicates the process exited with a non-zero exit code.
    /// </summary>
    NonZeroExit = 2,

    /// <summary>
    /// Indicates the process timed out.
    /// </summary>
    Timeout = 3,

    /// <summary>
    /// Indicates the process was canceled.
    /// </summary>
    Cancelled = 4
}
