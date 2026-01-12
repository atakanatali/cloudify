namespace Cloudify.Application.Ports;

/// <summary>
/// Represents an error raised when port allocation fails.
/// </summary>
public sealed class PortAllocationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PortAllocationException"/> class.
    /// </summary>
    /// <param name="message">The error message describing the allocation failure.</param>
    public PortAllocationException(string message)
        : base(message)
    {
    }
}
