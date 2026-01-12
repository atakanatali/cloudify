namespace Cloudify.Domain.Models;

/// <summary>
/// Represents resource capacity limits for an environment, including CPU, memory, and storage.
/// </summary>
public sealed class ResourceQuota
{
    /// <summary>
    /// Gets the CPU cores allowed for the environment.
    /// </summary>
    public int CpuCores { get; }

    /// <summary>
    /// Gets the memory limit in gigabytes for the environment.
    /// </summary>
    public int MemoryGb { get; }

    /// <summary>
    /// Gets the storage limit in gigabytes for the environment.
    /// </summary>
    public int StorageGb { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ResourceQuota"/> class.
    /// </summary>
    /// <param name="cpuCores">The CPU cores limit.</param>
    /// <param name="memoryGb">The memory limit in gigabytes.</param>
    /// <param name="storageGb">The storage limit in gigabytes.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when any limit is less than one.</exception>
    public ResourceQuota(int cpuCores, int memoryGb, int storageGb)
    {
        if (cpuCores < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(cpuCores), "CPU cores must be at least 1.");
        }

        if (memoryGb < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(memoryGb), "Memory must be at least 1 GB.");
        }

        if (storageGb < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(storageGb), "Storage must be at least 1 GB.");
        }

        CpuCores = cpuCores;
        MemoryGb = memoryGb;
        StorageGb = storageGb;
    }
}
