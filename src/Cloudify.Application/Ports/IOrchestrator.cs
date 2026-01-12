using Cloudify.Domain.Models;

namespace Cloudify.Application.Ports;

/// <summary>
/// Defines orchestration operations for environments and resources.
/// </summary>
public interface IOrchestrator
{
    /// <summary>
    /// Deploys the environment resources.
    /// </summary>
    /// <param name="environmentId">The environment identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task DeployEnvironmentAsync(Guid environmentId, CancellationToken cancellationToken);

    /// <summary>
    /// Starts a resource.
    /// </summary>
    /// <param name="resourceId">The resource identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task StartResourceAsync(Guid resourceId, CancellationToken cancellationToken);

    /// <summary>
    /// Stops a resource.
    /// </summary>
    /// <param name="resourceId">The resource identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task StopResourceAsync(Guid resourceId, CancellationToken cancellationToken);

    /// <summary>
    /// Restarts a resource.
    /// </summary>
    /// <param name="resourceId">The resource identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task RestartResourceAsync(Guid resourceId, CancellationToken cancellationToken);

    /// <summary>
    /// Scales a resource to the requested replica count.
    /// </summary>
    /// <param name="resourceId">The resource identifier.</param>
    /// <param name="replicas">The replica count.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task ScaleResourceAsync(Guid resourceId, int replicas, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves resource logs.
    /// </summary>
    /// <param name="resourceId">The resource identifier.</param>
    /// <param name="tail">The number of log lines to tail.</param>
    /// <param name="serviceName">The optional service name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The log output.</returns>
    Task<string> GetResourceLogsAsync(Guid resourceId, int tail, string? serviceName, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves the current health for a resource.
    /// </summary>
    /// <param name="resourceId">The resource identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The resource health snapshot.</returns>
    Task<ResourceHealth> GetResourceHealthAsync(Guid resourceId, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves resource status.
    /// </summary>
    /// <param name="resourceId">The resource identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The resource state.</returns>
    Task<ResourceState> GetResourceStatusAsync(Guid resourceId, CancellationToken cancellationToken);
}
