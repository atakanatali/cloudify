using Cloudify.Application.Dtos;

namespace Cloudify.Application.Ports;

/// <summary>
/// Defines the use case for restarting a resource.
/// </summary>
public interface IRestartResourceHandler
{
    /// <summary>
    /// Handles the restart resource request.
    /// </summary>
    /// <param name="request">The request payload.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result containing the response.</returns>
    Task<Result<RestartResourceResponse>> HandleAsync(RestartResourceRequest request, CancellationToken cancellationToken);
}
