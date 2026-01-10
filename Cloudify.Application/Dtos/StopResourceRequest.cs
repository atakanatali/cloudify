using Cloudify.Domain.Models;

namespace Cloudify.Application.Dtos;

/// <summary>
/// Represents a request to stop a resource.
/// </summary>
public sealed class StopResourceRequest
{
    /// <summary>
    /// Gets or sets the resource identifier.
    /// </summary>
    public Guid ResourceId { get; set; }
}

/// <summary>
/// Represents the response for stopping a resource.
/// </summary>
public sealed class StopResourceResponse
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
