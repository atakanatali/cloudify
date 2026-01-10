namespace Cloudify.Application.Dtos;

/// <summary>
/// Represents a request to delete a resource.
/// </summary>
public sealed class DeleteResourceRequest
{
    /// <summary>
    /// Gets or sets the resource identifier.
    /// </summary>
    public Guid ResourceId { get; set; }
}

/// <summary>
/// Represents the response for deleting a resource.
/// </summary>
public sealed class DeleteResourceResponse
{
    /// <summary>
    /// Gets or sets the deleted resource identifier.
    /// </summary>
    public Guid ResourceId { get; set; }
}
