namespace Cloudify.Application.Dtos;

/// <summary>
/// Represents a transport-friendly view of a cloud environment.
/// </summary>
public sealed class CloudEnvironmentDto
{
    /// <summary>
    /// Gets or sets the environment identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the environment name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the CPU cores limit.
    /// </summary>
    public int CpuCores { get; set; }

    /// <summary>
    /// Gets or sets the memory limit in gigabytes.
    /// </summary>
    public int MemoryGb { get; set; }

    /// <summary>
    /// Gets or sets the storage limit in gigabytes.
    /// </summary>
    public int StorageGb { get; set; }
}
