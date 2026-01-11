namespace Cloudify.Infrastructure.Persistence.Entities;

/// <summary>
/// Represents a persisted capacity profile record for a resource.
/// </summary>
public sealed class CapacityProfileRecord
{
    /// <summary>
    /// Gets or sets the resource identifier associated with the capacity profile.
    /// </summary>
    public Guid ResourceId { get; set; }

    /// <summary>
    /// Gets or sets the optional CPU limit for the resource.
    /// </summary>
    public int? CpuLimit { get; set; }

    /// <summary>
    /// Gets or sets the optional memory limit for the resource.
    /// </summary>
    public int? MemoryLimit { get; set; }

    /// <summary>
    /// Gets or sets the replica count for the resource.
    /// </summary>
    public int Replicas { get; set; }

    /// <summary>
    /// Gets or sets optional notes describing the capacity profile.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Gets or sets the resource navigation for the capacity profile.
    /// </summary>
    public ResourceRecord? Resource { get; set; }
}
