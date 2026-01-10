namespace Cloudify.Domain.Models;

/// <summary>
/// Represents compute capacity requirements for a resource.
/// </summary>
public sealed class CapacityProfile
{
    /// <summary>
    /// Gets the optional CPU limit.
    /// </summary>
    public int? CpuLimit { get; }

    /// <summary>
    /// Gets the optional memory limit in gigabytes.
    /// </summary>
    public int? MemoryLimit { get; }

    /// <summary>
    /// Gets the replica count.
    /// </summary>
    public int Replicas { get; }

    /// <summary>
    /// Gets optional notes describing the capacity profile.
    /// </summary>
    public string? Notes { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CapacityProfile"/> class.
    /// </summary>
    /// <param name="cpuLimit">The optional CPU limit.</param>
    /// <param name="memoryLimit">The optional memory limit in gigabytes.</param>
    /// <param name="replicas">The replica count.</param>
    /// <param name="notes">Optional notes.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when limits are invalid.</exception>
    /// <exception cref="ArgumentException">Thrown when notes are empty.</exception>
    public CapacityProfile(int? cpuLimit, int? memoryLimit, int replicas, string? notes)
    {
        if (cpuLimit is not null && cpuLimit <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(cpuLimit), "CPU limit must be greater than zero.");
        }

        if (memoryLimit is not null && memoryLimit <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(memoryLimit), "Memory limit must be greater than zero.");
        }

        if (replicas < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(replicas), "Replicas must be at least 1.");
        }

        if (notes is not null && string.IsNullOrWhiteSpace(notes))
        {
            throw new ArgumentException("Notes cannot be empty.", nameof(notes));
        }

        CpuLimit = cpuLimit;
        MemoryLimit = memoryLimit;
        Replicas = replicas;
        Notes = notes;
    }
}
