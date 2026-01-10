using Cloudify.Application.Dtos;

namespace Cloudify.Application.Ports;

/// <summary>
/// Defines the use case for stopping a resource.
/// </summary>
public interface IStopResourceHandler
{
    /// <summary>
    /// Handles the stop resource request.
    /// </summary>
    /// <param name="request">The request payload.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result containing the response.</returns>
    Task<Result<StopResourceResponse>> HandleAsync(StopResourceRequest request, CancellationToken cancellationToken);
}
