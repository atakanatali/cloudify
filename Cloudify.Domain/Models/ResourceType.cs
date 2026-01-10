namespace Cloudify.Domain.Models;

/// <summary>
/// Defines the supported resource types.
/// </summary>
public enum ResourceType
{
    /// <summary>
    /// Represents a Redis resource.
    /// </summary>
    Redis = 1,

    /// <summary>
    /// Represents a PostgreSQL resource.
    /// </summary>
    Postgres = 2,

    /// <summary>
    /// Represents a MongoDB resource.
    /// </summary>
    Mongo = 3,

    /// <summary>
    /// Represents a RabbitMQ resource.
    /// </summary>
    Rabbit = 4,

    /// <summary>
    /// Represents an application service resource.
    /// </summary>
    AppService = 5
}
