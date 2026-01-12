namespace Cloudify.Infrastructure.Processes;

/// <summary>
/// Represents errors that occur during process execution.
/// </summary>
public sealed class ProcessExecutionException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProcessExecutionException"/> class.
    /// </summary>
    /// <param name="command">The command executed.</param>
    /// <param name="arguments">The arguments passed.</param>
    /// <param name="result">The execution result.</param>
    public ProcessExecutionException(string command, IReadOnlyList<string> arguments, ProcessExecutionResult result)
        : base(BuildMessage(command, arguments, result))
    {
        Command = command;
        Arguments = arguments;
        Result = result;
    }

    /// <summary>
    /// Gets the command executed.
    /// </summary>
    public string Command { get; }

    /// <summary>
    /// Gets the arguments passed.
    /// </summary>
    public IReadOnlyList<string> Arguments { get; }

    /// <summary>
    /// Gets the execution result.
    /// </summary>
    public ProcessExecutionResult Result { get; }

    private static string BuildMessage(string command, IReadOnlyList<string> arguments, ProcessExecutionResult result)
    {
        string args = string.Join(' ', arguments);
        return $"Process '{command} {args}' failed with {result.ErrorCode} (exit {result.ExitCode}).";
    }
}
