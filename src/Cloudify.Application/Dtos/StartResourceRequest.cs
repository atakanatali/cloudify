using Cloudify.Domain.Models;

namespace Cloudify.Application.Dtos;

/// <summary>
/// Represents a request to start a resource.
/// </summary>
public sealed class StartResourceRequest
{
    /// <summary>
    /// Gets or sets the resource identifier.
    /// </summary>
    public Guid ResourceId { get; set; }
}

/// <summary>
/// Represents the response for starting a resource.
/// </summary>
public sealed class StartResourceResponse
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
