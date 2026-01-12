namespace Cloudify.Domain.Models;

/// <summary>
/// Represents a logical cloud environment with its identity, name, and resource limits.
/// </summary>
public sealed class CloudEnvironment
{
    /// <summary>
    /// Gets the unique identifier for the environment.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Gets the display name of the environment.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the resource quota allocated to the environment.
    /// </summary>
    public ResourceQuota Quota { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CloudEnvironment"/> class.
    /// </summary>
    /// <param name="id">The environment identifier.</param>
    /// <param name="name">The environment name.</param>
    /// <param name="quota">The resource quota for the environment.</param>
    /// <exception cref="ArgumentException">Thrown when the name is empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown when the quota is null.</exception>
    public CloudEnvironment(Guid id, string name, ResourceQuota quota)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Environment name is required.", nameof(name));
        }

        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        Name = name;
        Quota = quota ?? throw new ArgumentNullException(nameof(quota));
    }
}
