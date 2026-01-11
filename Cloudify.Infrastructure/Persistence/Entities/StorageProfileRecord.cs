namespace Cloudify.Infrastructure.Persistence.Entities;

/// <summary>
/// Represents a persisted storage profile record for a resource.
/// </summary>
public sealed class StorageProfileRecord
{
    /// <summary>
    /// Gets or sets the resource identifier associated with the storage profile.
    /// </summary>
    public Guid ResourceId { get; set; }

    /// <summary>
    /// Gets or sets the volume name for the storage profile.
    /// </summary>
    public string VolumeName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the storage size in gigabytes.
    /// </summary>
    public int SizeGb { get; set; }

    /// <summary>
    /// Gets or sets the mount path for the storage volume.
    /// </summary>
    public string MountPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the volume is persistent.
    /// </summary>
    public bool IsPersistent { get; set; }

    /// <summary>
    /// Gets or sets the resource navigation for the storage profile.
    /// </summary>
    public ResourceRecord? Resource { get; set; }
}
