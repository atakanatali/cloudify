using Cloudify.Domain.Models;
using Xunit;

namespace Cloudify.Domain.Tests;

/// <summary>
/// Tests for <see cref="ResourceQuota"/> validation rules.
/// </summary>
public sealed class ResourceQuotaTests
{
    /// <summary>
    /// Ensures that invalid CPU values are rejected.
    /// </summary>
    [Fact]
    public void Constructor_WithInvalidCpu_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ResourceQuota(0, 1, 1));
    }

    /// <summary>
    /// Ensures that valid values construct a quota.
    /// </summary>
    [Fact]
    public void Constructor_WithValidValues_Succeeds()
    {
        var quota = new ResourceQuota(2, 4, 8);

        Assert.Equal(2, quota.CpuCores);
        Assert.Equal(4, quota.MemoryGb);
        Assert.Equal(8, quota.StorageGb);
    }
}
