namespace Cloudify.Domain.Models;

/// <summary>
/// Represents an application service resource instance.
/// </summary>
public sealed class AppServiceResource : Resource
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AppServiceResource"/> class.
    /// </summary>
    /// <param name="id">The resource identifier.</param>
    /// <param name="environmentId">The owning environment identifier.</param>
    /// <param name="name">The resource name.</param>
    /// <param name="state">The resource state.</param>
    /// <param name="createdAt">The creation timestamp.</param>
    /// <param name="capacityProfile">The capacity profile.</param>
    /// <param name="portPolicy">The port policy.</param>
    public AppServiceResource(
        Guid id,
        Guid environmentId,
        string name,
        ResourceState state,
        DateTimeOffset createdAt,
        CapacityProfile? capacityProfile,
        PortPolicy? portPolicy)
        : base(id, environmentId, name, ResourceType.AppService, state, createdAt, capacityProfile, portPolicy)
    {
    }
}
