using Cloudify.Domain.Models;

namespace Cloudify.Infrastructure.Persistence.Entities;

/// <summary>
/// Represents a persisted environment record.
/// </summary>
public sealed class EnvironmentRecord
{
    /// <summary>
    /// Gets or sets the unique identifier for the environment.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the owning resource group identifier.
    /// </summary>
    public Guid ResourceGroupId { get; set; }

    /// <summary>
    /// Gets or sets the logical name for the environment.
    /// </summary>
    public EnvironmentName Name { get; set; }

    /// <summary>
    /// Gets or sets the networking mode for the environment.
    /// </summary>
    public NetworkMode NetworkMode { get; set; }

    /// <summary>
    /// Gets or sets the optional base domain for the environment.
    /// </summary>
    public string? BaseDomain { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the environment was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the resource records in the environment.
    /// </summary>
    public List<ResourceRecord> Resources { get; set; } = new();

    /// <summary>
    /// Gets or sets the resource port allocations for the environment.
    /// </summary>
    public List<ResourcePortRecord> AllocatedPorts { get; set; } = new();

    /// <summary>
    /// Gets or sets the owning resource group navigation.
    /// </summary>
    public ResourceGroupRecord? ResourceGroup { get; set; }
}
