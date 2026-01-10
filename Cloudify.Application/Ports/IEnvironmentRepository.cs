using Cloudify.Domain.Models;

namespace Cloudify.Application.Ports;

/// <summary>
/// Defines persistence operations for cloud environments.
/// </summary>
public interface IEnvironmentRepository
{
    /// <summary>
    /// Gets all environments currently stored.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The environments collection.</returns>
    Task<IReadOnlyList<CloudEnvironment>> GetAllAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Adds a new environment to the store.
    /// </summary>
    /// <param name="environment">The environment to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task AddAsync(CloudEnvironment environment, CancellationToken cancellationToken);
}
