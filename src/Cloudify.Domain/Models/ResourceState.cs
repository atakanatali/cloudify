namespace Cloudify.Domain.Models;

/// <summary>
/// Represents the lifecycle state for a resource.
/// </summary>
public enum ResourceState
{
    /// <summary>
    /// Indicates the resource is provisioning.
    /// </summary>
    Provisioning = 1,

    /// <summary>
    /// Indicates the resource is running.
    /// </summary>
    Running = 2,

    /// <summary>
    /// Indicates the resource is stopped.
    /// </summary>
    Stopped = 3,

    /// <summary>
    /// Indicates the resource is in a failed state.
    /// </summary>
    Failed = 4,

    /// <summary>
    /// Indicates the resource has been deleted.
    /// </summary>
    Deleted = 5
}
