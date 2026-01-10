namespace Cloudify.Application.Dtos;

/// <summary>
/// Represents capacity configuration for a resource.
/// </summary>
public sealed class CapacityProfileDto
{
    /// <summary>
    /// Gets or sets the CPU limit for the resource.
    /// </summary>
    public int? CpuLimit { get; set; }

    /// <summary>
    /// Gets or sets the memory limit in gigabytes.
    /// </summary>
    public int? MemoryLimit { get; set; }

    /// <summary>
    /// Gets or sets the desired replica count.
    /// </summary>
    public int Replicas { get; set; } = 1;

    /// <summary>
    /// Gets or sets the optional notes.
    /// </summary>
    public string? Notes { get; set; }
}
