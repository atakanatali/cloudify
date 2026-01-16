using Cloudify.Application.Dtos;
using Cloudify.Domain.Models;
using Cloudify.Infrastructure.Ports;
using Cloudify.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;
using EnvironmentModel = Cloudify.Domain.Models.Environment;

namespace Cloudify.Infrastructure.Tests;

/// <summary>
/// Provides integration tests for the port allocator.
/// </summary>
public sealed class PortAllocatorIntegrationTests
{
    /// <summary>
    /// Verifies that allocating ports for many resources yields unique ports.
    /// </summary>
    /// <returns>A task that completes when the test is finished.</returns>
    [Fact]
    public async Task AllocateManyPortsEnsuresUniquenessAsync()
    {
        await using SqliteConnection connection = CreateInMemoryConnection();
        await connection.OpenAsync();

        await using CloudifyDbContext context = CreateContext(connection);
        await InitializeAsync(context);

        var stateStore = new EfStateStore(context);
        var allocator = new PortAllocator(stateStore);

        Guid resourceGroupId = Guid.NewGuid();
        var resourceGroup = new ResourceGroup(resourceGroupId, "rg-ports", DateTimeOffset.UtcNow);
        await stateStore.AddResourceGroupAsync(resourceGroup, CancellationToken.None);

        var environment = new EnvironmentModel(
            Guid.NewGuid(),
            resourceGroupId,
            EnvironmentName.Test,
            NetworkMode.Bridge,
            null,
            DateTimeOffset.UtcNow);

        await stateStore.AddEnvironmentAsync(environment, CancellationToken.None);

        var allocatedPorts = new List<int>();
        for (int index = 0; index < 60; index++)
        {
            var resource = new AppServiceResource(
                Guid.NewGuid(),
                environment.Id,
                $"app-{index}",
                ResourceState.Provisioning,
                DateTimeOffset.UtcNow,
                null,
                "nginx:latest",
                null,
                null);

            await stateStore.AddResourceAsync(resource, CancellationToken.None);

            PortAllocationResultDto allocation = await allocator.AllocateAsync(
                environment.Id,
                ResourceType.AppService,
                null,
                CancellationToken.None);

            bool assigned = await stateStore.AssignPortAsync(environment.Id, resource.Id, allocation.Port, CancellationToken.None);
            Assert.True(assigned);

            allocatedPorts.Add(allocation.Port);
        }

        Assert.Equal(allocatedPorts.Count, allocatedPorts.Distinct().Count());
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
