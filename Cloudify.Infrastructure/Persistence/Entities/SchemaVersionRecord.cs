namespace Cloudify.Infrastructure.Persistence.Entities;

/// <summary>
/// Represents the persisted schema version record for the database.
/// </summary>
public sealed class SchemaVersionRecord
{
    /// <summary>
    /// Gets or sets the identifier for the schema version row.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the current schema version number.
    /// </summary>
    public int Version { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the schema version was applied.
    /// </summary>
    public DateTimeOffset AppliedAt { get; set; }
}
