namespace Cloudify.Domain.Models;

/// <summary>
/// Represents a unified health status for resources.
/// </summary>
public enum HealthStatus
{
    /// <summary>
    /// Indicates the resource health is healthy.
    /// </summary>
    Healthy = 1,

    /// <summary>
    /// Indicates the resource health is unhealthy.
    /// </summary>
    Unhealthy = 2,

    /// <summary>
    /// Indicates the resource health is unknown.
    /// </summary>
    Unknown = 3
}
