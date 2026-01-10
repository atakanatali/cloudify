namespace Cloudify.Application.Dtos;

/// <summary>
/// Represents a request to scale a resource.
/// </summary>
public sealed class ScaleResourceRequest
{
    /// <summary>
    /// Gets or sets the resource identifier.
    /// </summary>
    public Guid ResourceId { get; set; }

    /// <summary>
    /// Gets or sets the desired replica count.
    /// </summary>
    public int Replicas { get; set; }
}

/// <summary>
/// Represents the response for scaling a resource.
/// </summary>
public sealed class ScaleResourceResponse
{
    /// <summary>
    /// Gets or sets the resource identifier.
    /// </summary>
    public Guid ResourceId { get; set; }

    /// <summary>
    /// Gets or sets the updated replica count.
    /// </summary>
    public int Replicas { get; set; }
}
