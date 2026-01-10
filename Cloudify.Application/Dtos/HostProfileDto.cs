namespace Cloudify.Application.Dtos;

/// <summary>
/// Represents host system capacity information.
/// </summary>
public sealed class HostProfileDto
{
    /// <summary>
    /// Gets or sets the CPU core count.
    /// </summary>
    public int CpuCount { get; set; }

    /// <summary>
    /// Gets or sets the total memory in gigabytes.
    /// </summary>
    public int TotalMemoryGb { get; set; }

    /// <summary>
    /// Gets or sets the storage hint string.
    /// </summary>
    public string? StorageHint { get; set; }
}
