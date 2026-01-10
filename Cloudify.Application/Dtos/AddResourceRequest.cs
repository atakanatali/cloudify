using Cloudify.Domain.Models;

namespace Cloudify.Application.Dtos;

/// <summary>
/// Represents a request to add a resource to an environment.
/// </summary>
public sealed class AddResourceRequest
{
    /// <summary>
    /// Gets or sets the environment identifier.
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
    /// Gets or sets the requested capacity profile.
    /// </summary>
    public CapacityProfileDto? CapacityProfile { get; set; }

    /// <summary>
    /// Gets or sets the requested storage profile.
    /// </summary>
    public StorageProfileDto? StorageProfile { get; set; }

    /// <summary>
    /// Gets or sets the port policy.
    /// </summary>
    public PortPolicyDto? PortPolicy { get; set; }

    /// <summary>
    /// Gets or sets the requested primary port.
    /// </summary>
    public int? RequestedPort { get; set; }
}

/// <summary>
/// Represents the response for adding a resource.
/// </summary>
public sealed class AddResourceResponse
{
    /// <summary>
    /// Gets or sets the resource summary.
    /// </summary>
    public ResourceSummaryDto Resource { get; set; } = new();
}
