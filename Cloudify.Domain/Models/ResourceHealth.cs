namespace Cloudify.Domain.Models;

/// <summary>
/// Represents a unified health snapshot for a resource.
/// </summary>
public sealed class ResourceHealth
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ResourceHealth"/> class.
    /// </summary>
    /// <param name="state">The resource runtime state.</param>
    /// <param name="status">The computed health status.</param>
    public ResourceHealth(ResourceState state, HealthStatus status)
    {
        State = state;
        Status = status;
    }

    /// <summary>
    /// Gets the resource runtime state.
    /// </summary>
    public ResourceState State { get; }

    /// <summary>
    /// Gets the computed health status.
    /// </summary>
    public HealthStatus Status { get; }
}
