namespace Cloudify.Domain.Models;

/// <summary>
/// Represents a MongoDB resource instance.
/// </summary>
public sealed class MongoResource : Resource
{
    /// <summary>
    /// Gets the storage profile for the MongoDB resource.
    /// </summary>
    public StorageProfile StorageProfile { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MongoResource"/> class.
    /// </summary>
    /// <param name="id">The resource identifier.</param>
    /// <param name="environmentId">The owning environment identifier.</param>
    /// <param name="name">The resource name.</param>
    /// <param name="state">The resource state.</param>
    /// <param name="createdAt">The creation timestamp.</param>
    /// <param name="capacityProfile">The capacity profile.</param>
    /// <param name="storageProfile">The storage profile.</param>
    /// <param name="portPolicy">The port policy.</param>
    /// <exception cref="ArgumentNullException">Thrown when the storage profile is null.</exception>
    public MongoResource(
        Guid id,
        Guid environmentId,
        string name,
        ResourceState state,
        DateTimeOffset createdAt,
        CapacityProfile? capacityProfile,
        StorageProfile storageProfile,
        PortPolicy? portPolicy)
        : base(id, environmentId, name, ResourceType.Mongo, state, createdAt, capacityProfile, portPolicy)
    {
        StorageProfile = storageProfile ?? throw new ArgumentNullException(nameof(storageProfile));
    }
}
