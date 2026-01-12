using Cloudify.Infrastructure.Persistence.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Cloudify.Infrastructure.Persistence;

/// <summary>
/// Ensures the Cloudify database schema is created and versioned deterministically.
/// </summary>
public sealed class CloudifyDatabaseInitializer
{
    /// <summary>
    /// Defines the current schema version for the Cloudify database.
    /// </summary>
    private const int CurrentSchemaVersion = 1;

    /// <summary>
    /// Stores the database context used for initialization.
    /// </summary>
    private readonly CloudifyDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="CloudifyDatabaseInitializer"/> class.
    /// </summary>
    /// <param name="dbContext">The database context used for initialization.</param>
    public CloudifyDatabaseInitializer(CloudifyDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    /// <summary>
    /// Ensures the database file directory exists, schema is created, and versioned.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when initialization is done.</returns>
    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        EnsureDataDirectoryExists();
        await _dbContext.Database.EnsureCreatedAsync(cancellationToken);
        await EnsureSchemaVersionAsync(cancellationToken);
    }

    /// <summary>
    /// Ensures the database directory exists when using a file-based SQLite database.
    /// </summary>
    private void EnsureDataDirectoryExists()
    {
        string? directory = GetDatabaseDirectoryPath();
        if (string.IsNullOrWhiteSpace(directory))
        {
            return;
        }

        Directory.CreateDirectory(directory);
    }

    /// <summary>
    /// Resolves the directory path for the configured SQLite data source.
    /// </summary>
    /// <returns>The directory path or null when not applicable.</returns>
    private string? GetDatabaseDirectoryPath()
    {
        string connectionString = _dbContext.Database.GetDbConnection().ConnectionString;
        var builder = new SqliteConnectionStringBuilder(connectionString);

        if (string.IsNullOrWhiteSpace(builder.DataSource) || string.Equals(builder.DataSource, ":memory:", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        string fullPath = Path.GetFullPath(builder.DataSource);
        return Path.GetDirectoryName(fullPath);
    }

    /// <summary>
    /// Ensures the schema version is recorded and matches the expected version.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the schema version is validated.</returns>
    private async Task EnsureSchemaVersionAsync(CancellationToken cancellationToken)
    {
        SchemaVersionRecord? existing = await _dbContext.SchemaVersions.SingleOrDefaultAsync(cancellationToken);
        if (existing is null)
        {
            var record = new SchemaVersionRecord
            {
                Id = 1,
                Version = CurrentSchemaVersion,
                AppliedAt = DateTimeOffset.UtcNow,
            };

            _dbContext.SchemaVersions.Add(record);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return;
        }

        if (existing.Version != CurrentSchemaVersion)
        {
            throw new InvalidOperationException($"Schema version mismatch. Expected {CurrentSchemaVersion} but found {existing.Version}.");
        }
    }
}
