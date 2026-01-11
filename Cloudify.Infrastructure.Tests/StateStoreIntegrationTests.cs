using Cloudify.Domain.Models;
using Cloudify.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;
using EnvironmentModel = Cloudify.Domain.Models.Environment;

namespace Cloudify.Infrastructure.Tests;

/// <summary>
/// Provides integration tests for the EF Core-backed state store.
/// </summary>
public sealed class StateStoreIntegrationTests
{
    /// <summary>
    /// Verifies that resource groups and environments can be created and retrieved.
    /// </summary>
    /// <returns>A task that completes when the test is finished.</returns>
    [Fact]
    public async Task CreateAndGetResourceGroupAndEnvironmentAsync()
    {
        await using SqliteConnection connection = CreateInMemoryConnection();
        await connection.OpenAsync();

        await using CloudifyDbContext context = CreateContext(connection);
        await InitializeAsync(context);

        var stateStore = new EfStateStore(context);
        var resourceGroup = new ResourceGroup(Guid.NewGuid(), "rg-core", DateTimeOffset.UtcNow, new Dictionary<string, string>
        {
            ["owner"] = "platform",
        });

        await stateStore.AddResourceGroupAsync(resourceGroup, CancellationToken.None);

        IReadOnlyList<ResourceGroup> groups = await stateStore.ListResourceGroupsAsync(CancellationToken.None);
        ResourceGroup? fetchedGroup = await stateStore.GetResourceGroupAsync(resourceGroup.Id, CancellationToken.None);

        Assert.Single(groups);
        Assert.NotNull(fetchedGroup);
        Assert.Equal(resourceGroup.Name, fetchedGroup!.Name);
        Assert.Equal("platform", fetchedGroup.Tags["owner"]);

        var environment = new EnvironmentModel(
            Guid.NewGuid(),
            resourceGroup.Id,
            EnvironmentName.Dev,
            NetworkMode.Bridge,
            "dev.cloudify.local",
            DateTimeOffset.UtcNow);

        await stateStore.AddEnvironmentAsync(environment, CancellationToken.None);

        EnvironmentModel? fetchedEnvironment = await stateStore.GetEnvironmentAsync(environment.Id, CancellationToken.None);
        IReadOnlyList<EnvironmentModel> environments = await stateStore.ListEnvironmentsAsync(resourceGroup.Id, CancellationToken.None);

        Assert.NotNull(fetchedEnvironment);
        Assert.Equal(environment.Name, fetchedEnvironment!.Name);
        Assert.Single(environments);
        Assert.Equal(environment.Id, environments[0].Id);
    }

    /// <summary>
    /// Verifies that resources and port allocations are persisted and retrieved.
    /// </summary>
    /// <returns>A task that completes when the test is finished.</returns>
    [Fact]
    public async Task CreateAndGetResourceWithPortsAsync()
    {
        await using SqliteConnection connection = CreateInMemoryConnection();
        await connection.OpenAsync();

        await using CloudifyDbContext context = CreateContext(connection);
        await InitializeAsync(context);

        var stateStore = new EfStateStore(context);
        Guid resourceGroupId = Guid.NewGuid();
        var resourceGroup = new ResourceGroup(resourceGroupId, "rg-data", DateTimeOffset.UtcNow);
        await stateStore.AddResourceGroupAsync(resourceGroup, CancellationToken.None);

        var environment = new EnvironmentModel(
            Guid.NewGuid(),
            resourceGroupId,
            EnvironmentName.Test,
            NetworkMode.Bridge,
            null,
            DateTimeOffset.UtcNow);

        await stateStore.AddEnvironmentAsync(environment, CancellationToken.None);

        var capacityProfile = new CapacityProfile(2, 4, 2, "scale-test");
        var storageProfile = new StorageProfile("pg-data", 20, "/var/lib/postgresql/data", true);
        var portPolicy = new PortPolicy(new[] { 5432, 5433 });

        var resource = new PostgresResource(
            Guid.NewGuid(),
            environment.Id,
            "orders-db",
            ResourceState.Provisioning,
            DateTimeOffset.UtcNow,
            capacityProfile,
            storageProfile,
            portPolicy);

        await stateStore.AddResourceAsync(resource, CancellationToken.None);

        Resource? fetched = await stateStore.GetResourceAsync(resource.Id, CancellationToken.None);
        IReadOnlyList<Resource> resources = await stateStore.ListResourcesAsync(environment.Id, CancellationToken.None);

        Assert.NotNull(fetched);
        Assert.Single(resources);
        Assert.Equal(resource.Id, fetched!.Id);
        Assert.IsType<PostgresResource>(fetched);

        var fetchedPostgres = (PostgresResource)fetched;
        Assert.Equal(storageProfile.VolumeName, fetchedPostgres.StorageProfile.VolumeName);
        Assert.Equal(capacityProfile.Replicas, fetchedPostgres.CapacityProfile!.Replicas);
        Assert.NotNull(fetchedPostgres.PortPolicy);
        Assert.Contains(5432, fetchedPostgres.PortPolicy!.ExposedPorts);

        await stateStore.AssignPortAsync(environment.Id, resource.Id, 15432, CancellationToken.None);

        IReadOnlyList<int> ports = await stateStore.ListAllocatedPortsAsync(environment.Id, CancellationToken.None);

        Assert.Single(ports);
        Assert.Contains(15432, ports);

        await stateStore.RemovePortsAsync(environment.Id, resource.Id, CancellationToken.None);
        ports = await stateStore.ListAllocatedPortsAsync(environment.Id, CancellationToken.None);

        Assert.Empty(ports);
    }

    /// <summary>
    /// Creates an in-memory SQLite connection for testing.
    /// </summary>
    /// <returns>The initialized SQLite connection.</returns>
    private static SqliteConnection CreateInMemoryConnection()
    {
        return new SqliteConnection("DataSource=:memory:");
    }

    /// <summary>
    /// Creates the database context for the given connection.
    /// </summary>
    /// <param name="connection">The SQLite connection.</param>
    /// <returns>The configured database context.</returns>
    private static CloudifyDbContext CreateContext(SqliteConnection connection)
    {
        DbContextOptions<CloudifyDbContext> options = new DbContextOptionsBuilder<CloudifyDbContext>()
            .UseSqlite(connection)
            .Options;

        return new CloudifyDbContext(options);
    }

    /// <summary>
    /// Initializes the database schema for the provided context.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <returns>A task that completes when initialization is finished.</returns>
    private static async Task InitializeAsync(CloudifyDbContext context)
    {
        var initializer = new CloudifyDatabaseInitializer(context);
        await initializer.InitializeAsync(CancellationToken.None);
    }
}
