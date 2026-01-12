using System.Diagnostics;
using System.IO;

namespace Cloudify.Infrastructure.Processes;

/// <summary>
/// Provides process execution utilities.
/// </summary>
public sealed class ProcessRunner
{
    /// <summary>
    /// Executes a process asynchronously.
    /// </summary>
    /// <param name="request">The execution request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The execution result.</returns>
    public async Task<ProcessExecutionResult> RunAsync(ProcessExecutionRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.FileName))
        {
            throw new ArgumentException("FileName is required.", nameof(request));
        }

        using var process = new Process();
        process.StartInfo = BuildStartInfo(request);

        if (!process.Start())
        {
            return new ProcessExecutionResult
            {
                ErrorCode = ProcessErrorCode.StartFailed,
                StandardOutput = string.Empty,
                StandardError = string.Empty
            };
        }

        Task<string> outputTask = process.StandardOutput.ReadToEndAsync();
        Task<string> errorTask = process.StandardError.ReadToEndAsync();

        using var timeoutCts = request.Timeout.HasValue
            ? new CancellationTokenSource(request.Timeout.Value)
            : new CancellationTokenSource();
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

        try
        {
            await process.WaitForExitAsync(linkedCts.Token);
        }
        catch (OperationCanceledException)
        {
            TryTerminateProcess(process);
            string output = await SafeReadAsync(outputTask);
            string error = await SafeReadAsync(errorTask);
            return new ProcessExecutionResult
            {
                ErrorCode = timeoutCts.IsCancellationRequested && !cancellationToken.IsCancellationRequested
                    ? ProcessErrorCode.Timeout
                    : ProcessErrorCode.Cancelled,
                StandardOutput = output,
                StandardError = error
            };
        }

        string standardOutput = await SafeReadAsync(outputTask);
        string standardError = await SafeReadAsync(errorTask);

        return new ProcessExecutionResult
        {
            ExitCode = process.ExitCode,
            ErrorCode = process.ExitCode == 0 ? ProcessErrorCode.None : ProcessErrorCode.NonZeroExit,
            StandardOutput = standardOutput,
            StandardError = standardError
        };
    }

    private static ProcessStartInfo BuildStartInfo(ProcessExecutionRequest request)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = request.FileName,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        if (!string.IsNullOrWhiteSpace(request.WorkingDirectory))
        {
            startInfo.WorkingDirectory = request.WorkingDirectory;
        }

        foreach (string argument in request.Arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        return startInfo;
    }

    private static void TryTerminateProcess(Process process)
    {
        if (process.HasExited)
        {
            return;
        }

        try
        {
            process.Kill(entireProcessTree: true);
        }
        catch (InvalidOperationException)
        {
        }
    }

    private static async Task<string> SafeReadAsync(Task<string> task)
    {
        try
        {
            return await task;
        }
        catch (IOException)
        {
            return string.Empty;
        }
        catch (ObjectDisposedException)
        {
            return string.Empty;
        }
    }
}
