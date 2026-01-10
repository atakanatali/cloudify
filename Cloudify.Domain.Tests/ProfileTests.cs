using Cloudify.Domain.Models;
using Xunit;

namespace Cloudify.Domain.Tests;

/// <summary>
/// Tests for capacity, storage, and port profile guards.
/// </summary>
public sealed class ProfileTests
{
    /// <summary>
    /// Ensures capacity profiles reject invalid replicas.
    /// </summary>
    [Fact]
    public void CapacityProfile_WithInvalidReplicas_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new CapacityProfile(1, 2, 0, null));
    }

    /// <summary>
    /// Ensures storage profiles require a positive size.
    /// </summary>
    [Fact]
    public void StorageProfile_WithInvalidSize_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new StorageProfile("volume", 0, "/data", true));
    }

    /// <summary>
    /// Ensures port policies reject invalid port numbers.
    /// </summary>
    [Fact]
    public void PortPolicy_WithInvalidPorts_Throws()
    {
        Assert.Throws<ArgumentException>(() => new PortPolicy(new[] { 0 }));
    }
}
