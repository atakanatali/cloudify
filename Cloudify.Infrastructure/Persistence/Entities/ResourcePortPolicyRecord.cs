namespace Cloudify.Infrastructure.Persistence.Entities;

/// <summary>
/// Represents a persisted port policy entry for a resource.
/// </summary>
public sealed class ResourcePortPolicyRecord
{
    /// <summary>
    /// Gets or sets the resource identifier for the port policy entry.
    /// </summary>
    public Guid ResourceId { get; set; }

    /// <summary>
    /// Gets or sets the declared exposed port.
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// Gets or sets the resource navigation for the port policy entry.
    /// </summary>
    public ResourceRecord? Resource { get; set; }
}
