using Cloudify.Domain.Models;
using Xunit;

namespace Cloudify.Domain.Tests;

/// <summary>
/// Tests for <see cref="ResourceGroup"/> invariants.
/// </summary>
public sealed class ResourceGroupTests
{
    /// <summary>
    /// Ensures a resource group requires a name.
    /// </summary>
    [Fact]
    public void Constructor_WithEmptyName_Throws()
    {
        Assert.Throws<ArgumentException>(() => new ResourceGroup(Guid.NewGuid(), " ", DateTimeOffset.UtcNow));
    }

    /// <summary>
    /// Ensures environments must belong to the resource group when added.
    /// </summary>
    [Fact]
    public void AddEnvironment_WithMismatchedGroup_Throws()
    {
        var groupId = Guid.NewGuid();
        var resourceGroup = new ResourceGroup(groupId, "core", DateTimeOffset.UtcNow);

        var environment = new Environment(
            Guid.NewGuid(),
            Guid.NewGuid(),
            EnvironmentName.Test,
            NetworkMode.Bridge,
            null,
            DateTimeOffset.UtcNow);

        Assert.Throws<InvalidOperationException>(() => resourceGroup.AddEnvironment(environment));
    }
}
