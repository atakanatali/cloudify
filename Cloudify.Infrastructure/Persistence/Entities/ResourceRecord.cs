using Cloudify.Domain.Models;

namespace Cloudify.Infrastructure.Persistence.Entities;

/// <summary>
/// Represents a persisted resource record with common fields.
/// </summary>
public abstract class ResourceRecord
{
    /// <summary>
    /// Gets or sets the unique identifier for the resource.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the owning environment identifier.
    /// </summary>
    public Guid EnvironmentId { get; set; }

    /// <summary>
    /// Gets or sets the resource name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the resource type discriminator.
    /// </summary>
    public ResourceType ResourceType { get; set; }

    /// <summary>
    /// Gets or sets the current lifecycle state for the resource.
    /// </summary>
    public ResourceState State { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the resource was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the optional capacity profile record for the resource.
    /// </summary>
    public CapacityProfileRecord? CapacityProfile { get; set; }

    /// <summary>
    /// Gets or sets the optional storage profile record for the resource.
    /// </summary>
    public StorageProfileRecord? StorageProfile { get; set; }

    /// <summary>
    /// Gets or sets the optional credential profile record for the resource.
    /// </summary>
    public CredentialProfileRecord? CredentialProfile { get; set; }

    /// <summary>
    /// Gets or sets the declared port policy records for the resource.
    /// </summary>
    public List<ResourcePortPolicyRecord> PortPolicies { get; set; } = new();

    /// <summary>
    /// Gets or sets the allocated port records for the resource.
    /// </summary>
    public List<ResourcePortRecord> AllocatedPorts { get; set; } = new();

    /// <summary>
    /// Gets or sets the owning environment navigation.
    /// </summary>
    public EnvironmentRecord? Environment { get; set; }

    /// <summary>
    /// Gets or sets the container image for application services.
    /// </summary>
    public string? AppImage { get; set; }

    /// <summary>
    /// Gets or sets the optional HTTP health endpoint path for application services.
    /// </summary>
    public string? AppHealthEndpointPath { get; set; }
}

/// <summary>
/// Represents a persisted Redis resource record.
/// </summary>
public sealed class RedisResourceRecord : ResourceRecord
{
}

/// <summary>
/// Represents a persisted PostgreSQL resource record.
/// </summary>
public sealed class PostgresResourceRecord : ResourceRecord
{
}

/// <summary>
/// Represents a persisted MongoDB resource record.
/// </summary>
public sealed class MongoResourceRecord : ResourceRecord
{
}

/// <summary>
/// Represents a persisted RabbitMQ resource record.
/// </summary>
public sealed class RabbitResourceRecord : ResourceRecord
{
}

/// <summary>
/// Represents a persisted application service resource record.
/// </summary>
public sealed class AppServiceResourceRecord : ResourceRecord
{
}
