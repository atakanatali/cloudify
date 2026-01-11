using Cloudify.Domain.Models;

namespace Cloudify.Ui.Models;

/// <summary>
/// Provides UI defaults for resource types.
/// </summary>
public static class ResourceDefaults
{
    /// <summary>
    /// Gets a map of default ports per resource type.
    /// </summary>
    public static IReadOnlyDictionary<ResourceType, int[]> DefaultPorts { get; } = new Dictionary<ResourceType, int[]>
    {
        [ResourceType.Redis] = new[] { 6379 },
        [ResourceType.Postgres] = new[] { 5432 },
        [ResourceType.Mongo] = new[] { 27017 },
        [ResourceType.Rabbit] = new[] { 5672, 15672 },
        [ResourceType.AppService] = new[] { 8080, 5000 },
    };
}
