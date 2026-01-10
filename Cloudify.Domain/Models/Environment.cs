namespace Cloudify.Domain.Models;

/// <summary>
/// Represents a logical environment within a resource group.
/// </summary>
public sealed class Environment
{
    private readonly List<Resource> _resources;

    /// <summary>
    /// Gets the environment identifier.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Gets the resource group identifier that owns the environment.
    /// </summary>
    public Guid ResourceGroupId { get; }

    /// <summary>
    /// Gets the environment name.
    /// </summary>
    public EnvironmentName Name { get; }

    /// <summary>
    /// Gets the networking mode used by the environment.
    /// </summary>
    public NetworkMode NetworkMode { get; }

    /// <summary>
    /// Gets the base domain name for environment services.
    /// </summary>
    public string? BaseDomain { get; }

    /// <summary>
    /// Gets the creation timestamp for the environment.
    /// </summary>
    public DateTimeOffset CreatedAt { get; }

    /// <summary>
    /// Gets the resources contained within the environment.
    /// </summary>
    public IReadOnlyCollection<Resource> Resources => _resources;

    /// <summary>
    /// Initializes a new instance of the <see cref="Environment"/> class.
    /// </summary>
    /// <param name="id">The environment identifier.</param>
    /// <param name="resourceGroupId">The resource group identifier.</param>
    /// <param name="name">The environment name.</param>
    /// <param name="networkMode">The network mode.</param>
    /// <param name="baseDomain">Optional base domain.</param>
    /// <param name="createdAt">The creation timestamp.</param>
    /// <param name="resources">Optional seed resources.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the creation timestamp is not specified.</exception>
    /// <exception cref="ArgumentException">Thrown when base domain is empty.</exception>
    public Environment(
        Guid id,
        Guid resourceGroupId,
        EnvironmentName name,
        NetworkMode networkMode,
        string? baseDomain,
        DateTimeOffset createdAt,
        IEnumerable<Resource>? resources = null)
    {
        if (createdAt == default)
        {
            throw new ArgumentOutOfRangeException(nameof(createdAt), "CreatedAt must be specified.");
        }

        if (baseDomain is not null && string.IsNullOrWhiteSpace(baseDomain))
        {
            throw new ArgumentException("Base domain cannot be empty.", nameof(baseDomain));
        }

        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        ResourceGroupId = resourceGroupId;
        Name = name;
        NetworkMode = networkMode == default ? NetworkMode.Bridge : networkMode;
        BaseDomain = baseDomain;
        CreatedAt = createdAt;
        _resources = resources is null ? new List<Resource>() : new List<Resource>(resources);
    }

    /// <summary>
    /// Adds a resource to the environment while enforcing unique resource names.
    /// </summary>
    /// <param name="resource">The resource to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when the resource is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the resource belongs to another environment.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the resource name is already in use.</exception>
    public void AddResource(Resource resource)
    {
        if (resource is null)
        {
            throw new ArgumentNullException(nameof(resource));
        }

        if (resource.EnvironmentId != Id)
        {
            throw new InvalidOperationException("Resource does not belong to this environment.");
        }

        if (_resources.Any(existing => string.Equals(existing.Name, resource.Name, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("Resource name must be unique within the environment.");
        }

        _resources.Add(resource);
    }
}
