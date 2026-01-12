using Cloudify.Application.Ports;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Cloudify.Infrastructure.Persistence;

/// <summary>
/// Provides dependency injection helpers for Cloudify persistence services.
/// </summary>
public static class PersistenceServiceCollectionExtensions
{
    /// <summary>
    /// Registers Cloudify persistence services backed by SQLite.
    /// </summary>
    /// <param name="services">The service collection to register with.</param>
    /// <param name="databasePath">The SQLite database file path.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddCloudifyPersistence(this IServiceCollection services, string databasePath)
    {
        if (services is null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (string.IsNullOrWhiteSpace(databasePath))
        {
            throw new ArgumentException("Database path is required.", nameof(databasePath));
        }

        var builder = new SqliteConnectionStringBuilder
        {
            DataSource = databasePath,
        };

        services.AddDbContext<CloudifyDbContext>(options => options.UseSqlite(builder.ConnectionString));
        services.AddScoped<IStateStore, EfStateStore>();
        services.AddScoped<CloudifyDatabaseInitializer>();

        return services;
    }
}
