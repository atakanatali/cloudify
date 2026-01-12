using System.Globalization;
using Cloudify.Application.Dtos;
using Cloudify.Application.Ports;

namespace Cloudify.Infrastructure.SystemProfiles;

/// <summary>
/// Provides best-effort host capacity information for the current machine.
/// </summary>
public sealed class HostSystemProfileProvider : ISystemProfileProvider
{
    private const long BytesPerGb = 1024L * 1024L * 1024L;
    private const string LinuxMemInfoPath = "/proc/meminfo";

    /// <summary>
    /// Retrieves the host profile snapshot for UI consumption.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The host profile data transfer object.</returns>
    public Task<HostProfileDto> GetHostProfileAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        int cpuCount = Environment.ProcessorCount;
        int totalMemoryGb = TryGetTotalMemoryGb() ?? 0;
        DiskProfile diskProfile = GetDiskProfile();

        var profile = new HostProfileDto
        {
            CpuCount = cpuCount,
            TotalMemoryGb = totalMemoryGb,
            AvailableDiskGb = diskProfile.AvailableGb,
            StorageHint = diskProfile.Hint,
        };

        return Task.FromResult(profile);
    }

    /// <summary>
    /// Attempts to resolve total system memory in gigabytes.
    /// </summary>
    /// <returns>The memory in gigabytes or null when unavailable.</returns>
    private static int? TryGetTotalMemoryGb()
    {
        if (TryReadLinuxMemInfoGb(out int linuxGb))
        {
            return linuxGb;
        }

        long availableBytes = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes;
        if (availableBytes <= 0)
        {
            return null;
        }

        return (int)Math.Ceiling(availableBytes / (double)BytesPerGb);
    }

    /// <summary>
    /// Attempts to read Linux meminfo for total memory in gigabytes.
    /// </summary>
    /// <param name="totalGb">The resolved memory value.</param>
    /// <returns>True when memory data was found; otherwise, false.</returns>
    private static bool TryReadLinuxMemInfoGb(out int totalGb)
    {
        totalGb = 0;

        if (!OperatingSystem.IsLinux() || !File.Exists(LinuxMemInfoPath))
        {
            return false;
        }

        foreach (string line in File.ReadLines(LinuxMemInfoPath))
        {
            if (!line.StartsWith("MemTotal", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2 || !long.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out long kilobytes))
            {
                return false;
            }

            long bytes = kilobytes * 1024L;
            totalGb = (int)Math.Ceiling(bytes / (double)BytesPerGb);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Resolves the best-effort disk availability for the current host.
    /// </summary>
    /// <returns>The disk profile data for reporting.</returns>
    private static DiskProfile GetDiskProfile()
    {
        string? root = Path.GetPathRoot(AppContext.BaseDirectory);
        if (string.IsNullOrWhiteSpace(root))
        {
            return new DiskProfile(null, "Disk availability could not be detected.");
        }

        DriveInfo? drive = DriveInfo.GetDrives()
            .FirstOrDefault(candidate => candidate.IsReady && string.Equals(candidate.Name, root, StringComparison.OrdinalIgnoreCase));

        if (drive is null)
        {
            return new DiskProfile(null, $"Disk availability for {root} could not be detected.");
        }

        int availableGb = (int)Math.Floor(drive.AvailableFreeSpace / (double)BytesPerGb);
        string hint = $"Volume {drive.Name.TrimEnd(Path.DirectorySeparatorChar)}";

        return new DiskProfile(availableGb, hint);
    }

    /// <summary>
    /// Represents a disk availability snapshot for the host.
    /// </summary>
    private sealed class DiskProfile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DiskProfile"/> class.
        /// </summary>
        /// <param name="availableGb">The available storage in gigabytes.</param>
        /// <param name="hint">The storage hint.</param>
        public DiskProfile(int? availableGb, string? hint)
        {
            AvailableGb = availableGb;
            Hint = hint;
        }

        /// <summary>
        /// Gets the available storage in gigabytes.
        /// </summary>
        public int? AvailableGb { get; }

        /// <summary>
        /// Gets the storage hint string.
        /// </summary>
        public string? Hint { get; }
    }
}
