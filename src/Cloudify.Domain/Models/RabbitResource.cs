namespace Cloudify.Domain.Models;

/// <summary>
/// Represents a RabbitMQ resource instance.
/// </summary>
public sealed class RabbitResource : Resource
{
    /// <summary>
    /// Gets the storage profile for the RabbitMQ resource.
    /// </summary>
    public StorageProfile StorageProfile { get; }

    /// <summary>
    /// Gets the credential profile for the RabbitMQ resource.
    /// </summary>
    public CredentialProfile CredentialProfile { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RabbitResource"/> class.
    /// </summary>
    /// <param name="id">The resource identifier.</param>
    /// <param name="environmentId">The owning environment identifier.</param>
    /// <param name="name">The resource name.</param>
    /// <param name="state">The resource state.</param>
    /// <param name="createdAt">The creation timestamp.</param>
    /// <param name="capacityProfile">The capacity profile.</param>
    /// <param name="storageProfile">The storage profile.</param>
    /// <param name="credentialProfile">The credential profile.</param>
    /// <param name="portPolicy">The port policy.</param>
    /// <exception cref="ArgumentNullException">Thrown when the storage profile or credential profile is null.</exception>
    public RabbitResource(
        Guid id,
        Guid environmentId,
        string name,
        ResourceState state,
        DateTimeOffset createdAt,
        CapacityProfile? capacityProfile,
        StorageProfile storageProfile,
        CredentialProfile credentialProfile,
        PortPolicy? portPolicy)
        : base(id, environmentId, name, ResourceType.Rabbit, state, createdAt, capacityProfile, portPolicy)
    {
        StorageProfile = storageProfile ?? throw new ArgumentNullException(nameof(storageProfile));
        CredentialProfile = credentialProfile ?? throw new ArgumentNullException(nameof(credentialProfile));
    }
}
