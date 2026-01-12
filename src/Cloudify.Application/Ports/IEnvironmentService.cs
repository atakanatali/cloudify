using Cloudify.Application.Dtos;

namespace Cloudify.Application.Ports;

/// <summary>
/// Defines use-case operations for managing environments.
/// </summary>
public interface IEnvironmentService
{
    /// <summary>
    /// Retrieves all environments.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The list of environment DTOs.</returns>
    Task<IReadOnlyList<CloudEnvironmentDto>> GetAllAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Creates a new environment.
    /// </summary>
    /// <param name="request">The environment data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task CreateAsync(CloudEnvironmentDto request, CancellationToken cancellationToken);
}
