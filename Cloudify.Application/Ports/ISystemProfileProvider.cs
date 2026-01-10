using Cloudify.Application.Dtos;

namespace Cloudify.Application.Ports;

/// <summary>
/// Defines operations for retrieving system host profiles.
/// </summary>
public interface ISystemProfileProvider
{
    /// <summary>
    /// Retrieves the host profile information.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The host profile data.</returns>
    Task<HostProfileDto> GetHostProfileAsync(CancellationToken cancellationToken);
}
