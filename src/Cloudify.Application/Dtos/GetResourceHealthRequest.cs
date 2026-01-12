using Cloudify.Domain.Models;

namespace Cloudify.Application.Dtos;

/// <summary>
/// Represents a request to retrieve resource health.
/// </summary>
public sealed class GetResourceHealthRequest
{
    /// <summary>
    /// Gets or sets the resource identifier.
    /// </summary>
    public Guid ResourceId { get; set; }
}

/// <summary>
/// Represents the response for retrieving resource health.
/// </summary>
public sealed class GetResourceHealthResponse
{
    /// <summary>
    /// Gets or sets the resource identifier.
    /// </summary>
    public Guid ResourceId { get; set; }

    /// <summary>
    /// Gets or sets the resource state.
    /// </summary>
    public ResourceState State { get; set; }

    /// <summary>
    /// Gets or sets the computed health status.
    /// </summary>
    public HealthStatus HealthStatus { get; set; }
}
