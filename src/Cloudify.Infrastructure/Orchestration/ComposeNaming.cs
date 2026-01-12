using Cloudify.Domain.Models;

namespace Cloudify.Infrastructure.Orchestration;

/// <summary>
/// Provides deterministic naming helpers for Docker Compose artifacts.
/// </summary>
internal static class ComposeNaming
{
    /// <summary>
    /// Builds the Compose service name for the provided resource.
    /// </summary>
    /// <param name="resource">The resource to name.</param>
    /// <returns>The deterministic service name.</returns>
    public static string GetServiceName(Resource resource)
    {
        string shortId = GetResourceIdShort(resource.Id);
        string typeName = GetServiceTypeName(resource.ResourceType);
        return $"{typeName}-{shortId}";
    }

    /// <summary>
    /// Builds the volume name for the resource in the given environment.
    /// </summary>
    /// <param name="environmentId">The environment identifier.</param>
    /// <param name="resourceId">The resource identifier.</param>
    /// <returns>The deterministic volume name.</returns>
    public static string GetVolumeName(Guid environmentId, Guid resourceId)
    {
        string shortId = GetResourceIdShort(resourceId);
        return $"cloudify-{environmentId}-{shortId}-data";
    }

    /// <summary>
    /// Builds a short identifier segment from the resource identifier.
    /// </summary>
    /// <param name="resourceId">The resource identifier.</param>
    /// <returns>The short identifier segment.</returns>
    public static string GetResourceIdShort(Guid resourceId)
    {
        string normalized = resourceId.ToString("N");
        return normalized.Length >= 6 ? normalized[..6] : normalized;
    }

    /// <summary>
    /// Maps the resource type to a Compose service prefix.
    /// </summary>
    /// <param name="resourceType">The resource type.</param>
    /// <returns>The service prefix.</returns>
    public static string GetServiceTypeName(ResourceType resourceType)
    {
        return resourceType switch
        {
            ResourceType.Redis => "redis",
            ResourceType.Postgres => "postgres",
            ResourceType.Mongo => "mongo",
            ResourceType.Rabbit => "rabbitmq",
            ResourceType.AppService => "appservice",
            _ => "service",
        };
    }
}
