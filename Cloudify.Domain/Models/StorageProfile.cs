namespace Cloudify.Domain.Models;

/// <summary>
/// Represents storage requirements for a resource.
/// </summary>
public sealed class StorageProfile
{
    /// <summary>
    /// Gets the volume name.
    /// </summary>
    public string VolumeName { get; }

    /// <summary>
    /// Gets the storage size in gigabytes.
    /// </summary>
    public int SizeGb { get; }

    /// <summary>
    /// Gets the mount path for the volume.
    /// </summary>
    public string MountPath { get; }

    /// <summary>
    /// Gets a value indicating whether the volume is persistent.
    /// </summary>
    public bool IsPersistent { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="StorageProfile"/> class.
    /// </summary>
    /// <param name="volumeName">The volume name.</param>
    /// <param name="sizeGb">The size in gigabytes.</param>
    /// <param name="mountPath">The mount path.</param>
    /// <param name="isPersistent">Whether the volume is persistent.</param>
    /// <exception cref="ArgumentException">Thrown when required fields are empty.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when size is invalid.</exception>
    public StorageProfile(string volumeName, int sizeGb, string mountPath, bool isPersistent)
    {
        if (string.IsNullOrWhiteSpace(volumeName))
        {
            throw new ArgumentException("Volume name is required.", nameof(volumeName));
        }

        if (sizeGb < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(sizeGb), "SizeGb must be at least 1.");
        }

        if (string.IsNullOrWhiteSpace(mountPath))
        {
            throw new ArgumentException("Mount path is required.", nameof(mountPath));
        }

        VolumeName = volumeName;
        SizeGb = sizeGb;
        MountPath = mountPath;
        IsPersistent = isPersistent;
    }
}
