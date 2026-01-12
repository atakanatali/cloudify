using Cloudify.Domain.Models;

namespace Cloudify.Application.Dtos;

/// <summary>
/// Represents a summary view of a resource.
/// </summary>
public sealed class ResourceSummaryDto
{
    /// <summary>
    /// Gets or sets the resource identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the owning environment identifier.
    /// </summary>
    public Guid EnvironmentId { get; set; }

    /// <summary>
    /// Gets or sets the resource name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the resource type.
    /// </summary>
    public ResourceType ResourceType { get; set; }

    /// <summary>
    /// Gets or sets the resource state.
    /// </summary>
    public ResourceState State { get; set; }

    /// <summary>
    /// Gets or sets the computed health status for the resource.
    /// </summary>
    public HealthStatus HealthStatus { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the capacity profile.
    /// </summary>
    public CapacityProfileDto? CapacityProfile { get; set; }

    /// <summary>
    /// Gets or sets the storage profile.
    /// </summary>
    public StorageProfileDto? StorageProfile { get; set; }

    /// <summary>
    /// Gets or sets the port policy.
    /// </summary>
    public PortPolicyDto? PortPolicy { get; set; }

    /// <summary>
    /// Gets or sets the computed connection info for the resource.
    /// </summary>
    public ConnectionInfoDto? ConnectionInfo { get; set; }
}
