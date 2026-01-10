namespace Cloudify.Domain.Models;

/// <summary>
/// Represents a PostgreSQL resource instance.
/// </summary>
public sealed class PostgresResource : Resource
{
    /// <summary>
    /// Gets the storage profile for the PostgreSQL resource.
    /// </summary>
    public StorageProfile StorageProfile { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PostgresResource"/> class.
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
    public PostgresResource(
        Guid id,
        Guid environmentId,
        string name,
        ResourceState state,
        DateTimeOffset createdAt,
        CapacityProfile? capacityProfile,
        StorageProfile storageProfile,
        PortPolicy? portPolicy)
        : base(id, environmentId, name, ResourceType.Postgres, state, createdAt, capacityProfile, portPolicy)
    {
        StorageProfile = storageProfile ?? throw new ArgumentNullException(nameof(storageProfile));
    }
}
