namespace Cloudify.Infrastructure.Persistence.Entities;

/// <summary>
/// Represents a persisted resource group record.
/// </summary>
public sealed class ResourceGroupRecord
{
    /// <summary>
    /// Gets or sets the unique identifier for the resource group.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the display name for the resource group.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timestamp when the resource group was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the tag records associated with the resource group.
    /// </summary>
    public List<ResourceGroupTagRecord> Tags { get; set; } = new();

    /// <summary>
    /// Gets or sets the environment records owned by the resource group.
    /// </summary>
    public List<EnvironmentRecord> Environments { get; set; } = new();
}

/// <summary>
/// Represents a key/value tag persisted for a resource group.
/// </summary>
public sealed class ResourceGroupTagRecord
{
    /// <summary>
    /// Gets or sets the owning resource group identifier.
    /// </summary>
    public Guid ResourceGroupId { get; set; }

    /// <summary>
    /// Gets or sets the tag key.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the tag value.
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the parent resource group navigation.
    /// </summary>
    public ResourceGroupRecord? ResourceGroup { get; set; }
}
