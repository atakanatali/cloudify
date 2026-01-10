namespace Cloudify.Application.Dtos;

/// <summary>
/// Represents storage configuration for a resource.
/// </summary>
public sealed class StorageProfileDto
{
    /// <summary>
    /// Gets or sets the storage volume name.
    /// </summary>
    public string VolumeName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the storage size in gigabytes.
    /// </summary>
    public int SizeGb { get; set; }

    /// <summary>
    /// Gets or sets the mount path for the volume.
    /// </summary>
    public string MountPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the volume is persistent.
    /// </summary>
    public bool IsPersistent { get; set; }
}
