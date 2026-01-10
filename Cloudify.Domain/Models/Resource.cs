namespace Cloudify.Domain.Models;

/// <summary>
/// Represents a base resource within an environment.
/// </summary>
public abstract class Resource
{
    /// <summary>
    /// Gets the resource identifier.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Gets the owning environment identifier.
    /// </summary>
    public Guid EnvironmentId { get; }

    /// <summary>
    /// Gets the resource name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the resource type.
    /// </summary>
    public ResourceType ResourceType { get; }

    /// <summary>
    /// Gets the resource lifecycle state.
    /// </summary>
    public ResourceState State { get; private set; }

    /// <summary>
    /// Gets the creation timestamp for the resource.
    /// </summary>
    public DateTimeOffset CreatedAt { get; }

    /// <summary>
    /// Gets the capacity profile for the resource.
    /// </summary>
    public CapacityProfile? CapacityProfile { get; }

    /// <summary>
    /// Gets the port policy for the resource.
    /// </summary>
    public PortPolicy? PortPolicy { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Resource"/> class.
    /// </summary>
    /// <param name="id">The resource identifier.</param>
    /// <param name="environmentId">The owning environment identifier.</param>
    /// <param name="name">The resource name.</param>
    /// <param name="resourceType">The resource type.</param>
    /// <param name="state">The resource state.</param>
    /// <param name="createdAt">The creation timestamp.</param>
    /// <param name="capacityProfile">The capacity profile.</param>
    /// <param name="portPolicy">The port policy.</param>
    /// <exception cref="ArgumentException">Thrown when name is empty.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when createdAt is not specified.</exception>
    protected Resource(
        Guid id,
        Guid environmentId,
        string name,
        ResourceType resourceType,
        ResourceState state,
        DateTimeOffset createdAt,
        CapacityProfile? capacityProfile,
        PortPolicy? portPolicy)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Resource name is required.", nameof(name));
        }

        if (createdAt == default)
        {
            throw new ArgumentOutOfRangeException(nameof(createdAt), "CreatedAt must be specified.");
        }

        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        EnvironmentId = environmentId;
        Name = name;
        ResourceType = resourceType;
        State = state;
        CreatedAt = createdAt;
        CapacityProfile = capacityProfile;
        PortPolicy = portPolicy;
    }

    /// <summary>
    /// Updates the lifecycle state for the resource.
    /// </summary>
    /// <param name="state">The new state.</param>
    public void SetState(ResourceState state)
    {
        State = state;
    }
}
