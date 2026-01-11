using Cloudify.Domain.Models;
using Xunit;

namespace Cloudify.Domain.Tests;

/// <summary>
/// Tests for <see cref="Environment"/> behaviors.
/// </summary>
public sealed class EnvironmentTests
{
    /// <summary>
    /// Ensures resources must have unique names within an environment.
    /// </summary>
    [Fact]
    public void AddResource_WithDuplicateName_Throws()
    {
        var environmentId = Guid.NewGuid();
        var environment = new Environment(
            environmentId,
            Guid.NewGuid(),
            EnvironmentName.Prod,
            NetworkMode.Bridge,
            null,
            DateTimeOffset.UtcNow);

        var storage = new StorageProfile("redis-data", 10, "/data", true);

        environment.AddResource(new RedisResource(
            Guid.NewGuid(),
            environmentId,
            "cache",
            ResourceState.Running,
            DateTimeOffset.UtcNow,
            null,
            storage,
            null));

        Assert.Throws<InvalidOperationException>(() => environment.AddResource(new RedisResource(
            Guid.NewGuid(),
            environmentId,
            "CACHE",
            ResourceState.Running,
            DateTimeOffset.UtcNow,
            null,
            storage,
            null)));
    }

    /// <summary>
    /// Ensures resources must belong to the environment they are added to.
    /// </summary>
    [Fact]
    public void AddResource_WithMismatchedEnvironment_Throws()
    {
        var environment = new Environment(
            Guid.NewGuid(),
            Guid.NewGuid(),
            EnvironmentName.Dev,
            NetworkMode.Bridge,
            null,
            DateTimeOffset.UtcNow);

        var storage = new StorageProfile("db-data", 20, "/var/lib/postgres", true);
        var credentialProfile = new CredentialProfile("admin", "password");

        var resource = new PostgresResource(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "db",
            ResourceState.Provisioning,
            DateTimeOffset.UtcNow,
            null,
            storage,
            credentialProfile,
            null);

        Assert.Throws<InvalidOperationException>(() => environment.AddResource(resource));
    }
}
