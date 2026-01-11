namespace Cloudify.Domain.Models;

/// <summary>
/// Represents an application service resource instance.
/// </summary>
public sealed class AppServiceResource : Resource
{
    /// <summary>
    /// Gets the container image for the application service.
    /// </summary>
    public string Image { get; }

    /// <summary>
    /// Gets the optional HTTP health endpoint path for the application service.
    /// </summary>
    public string? HealthEndpointPath { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AppServiceResource"/> class.
    /// </summary>
    /// <param name="id">The resource identifier.</param>
    /// <param name="environmentId">The owning environment identifier.</param>
    /// <param name="name">The resource name.</param>
    /// <param name="state">The resource state.</param>
    /// <param name="createdAt">The creation timestamp.</param>
    /// <param name="capacityProfile">The capacity profile.</param>
    /// <param name="image">The container image to deploy.</param>
    /// <param name="portPolicy">The port policy.</param>
    /// <param name="healthEndpointPath">The optional HTTP health endpoint path.</param>
    /// <exception cref="ArgumentException">Thrown when the image is empty.</exception>
    public AppServiceResource(
        Guid id,
        Guid environmentId,
        string name,
        ResourceState state,
        DateTimeOffset createdAt,
        CapacityProfile? capacityProfile,
        string image,
        PortPolicy? portPolicy,
        string? healthEndpointPath)
        : base(id, environmentId, name, ResourceType.AppService, state, createdAt, capacityProfile, portPolicy)
    {
        if (string.IsNullOrWhiteSpace(image))
        {
            throw new ArgumentException("Image is required.", nameof(image));
        }

        Image = image;
        HealthEndpointPath = string.IsNullOrWhiteSpace(healthEndpointPath) ? null : healthEndpointPath.Trim();
    }
}
