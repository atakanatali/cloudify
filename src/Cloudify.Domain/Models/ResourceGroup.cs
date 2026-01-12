namespace Cloudify.Domain.Models;

/// <summary>
/// Represents a logical resource group that owns environments and shared metadata.
/// </summary>
public sealed class ResourceGroup
{
    private readonly List<Environment> _environments;
    private readonly Dictionary<string, string> _tags;

    /// <summary>
    /// Gets the resource group identifier.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Gets the resource group name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the creation timestamp for the resource group.
    /// </summary>
    public DateTimeOffset CreatedAt { get; }

    /// <summary>
    /// Gets the tags applied to the resource group.
    /// </summary>
    public IReadOnlyDictionary<string, string> Tags => _tags;

    /// <summary>
    /// Gets the environments owned by the resource group.
    /// </summary>
    public IReadOnlyCollection<Environment> Environments => _environments;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResourceGroup"/> class.
    /// </summary>
    /// <param name="id">The identifier for the resource group.</param>
    /// <param name="name">The name of the resource group.</param>
    /// <param name="createdAt">The timestamp when the resource group was created.</param>
    /// <param name="tags">Optional tags applied to the resource group.</param>
    /// <param name="environments">Optional environments to seed the group with.</param>
    /// <exception cref="ArgumentException">Thrown when the name is empty.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the creation timestamp is not specified.</exception>
    public ResourceGroup(
        Guid id,
        string name,
        DateTimeOffset createdAt,
        IDictionary<string, string>? tags = null,
        IEnumerable<Environment>? environments = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Resource group name is required.", nameof(name));
        }

        if (createdAt == default)
        {
            throw new ArgumentOutOfRangeException(nameof(createdAt), "CreatedAt must be specified.");
        }

        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        Name = name;
        CreatedAt = createdAt;
        _tags = tags is null ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) : new Dictionary<string, string>(tags, StringComparer.OrdinalIgnoreCase);
        _environments = environments is null ? new List<Environment>() : new List<Environment>(environments);
    }

    /// <summary>
    /// Adds a new environment to the resource group.
    /// </summary>
    /// <param name="environment">The environment to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when the environment is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the environment does not belong to the resource group.</exception>
    public void AddEnvironment(Environment environment)
    {
        if (environment is null)
        {
            throw new ArgumentNullException(nameof(environment));
        }

        if (environment.ResourceGroupId != Id)
        {
            throw new InvalidOperationException("Environment does not belong to this resource group.");
        }

        _environments.Add(environment);
    }
}
