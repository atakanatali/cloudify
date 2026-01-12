using Cloudify.Domain.Models;

namespace Cloudify.Application.Dtos;

/// <summary>
/// Represents a request to restart a resource.
/// </summary>
public sealed class RestartResourceRequest
{
    /// <summary>
    /// Gets or sets the resource identifier.
    /// </summary>
    public Guid ResourceId { get; set; }
}

/// <summary>
/// Represents the response for restarting a resource.
/// </summary>
public sealed class RestartResourceResponse
{
    /// <summary>
    /// Gets or sets the resource identifier.
    /// </summary>
    public Guid ResourceId { get; set; }

    /// <summary>
    /// Gets or sets the resource state.
    /// </summary>
    public ResourceState State { get; set; }
}
