namespace Cloudify.Infrastructure.Persistence.Entities;

/// <summary>
/// Represents a persisted resource port allocation record.
/// </summary>
public sealed class ResourcePortRecord
{
    /// <summary>
    /// Gets or sets the environment identifier for the allocated port.
    /// </summary>
    public Guid EnvironmentId { get; set; }

    /// <summary>
    /// Gets or sets the resource identifier for the allocated port.
    /// </summary>
    public Guid ResourceId { get; set; }

    /// <summary>
    /// Gets or sets the allocated port number.
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// Gets or sets the environment navigation for the allocation.
    /// </summary>
    public EnvironmentRecord? Environment { get; set; }

    /// <summary>
    /// Gets or sets the resource navigation for the allocation.
    /// </summary>
    public ResourceRecord? Resource { get; set; }
}
