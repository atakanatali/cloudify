using Cloudify.Application.Dtos;

namespace Cloudify.Application.Ports;

/// <summary>
/// Defines the use case for scaling a resource.
/// </summary>
public interface IScaleResourceHandler
{
    /// <summary>
    /// Handles the scale resource request.
    /// </summary>
    /// <param name="request">The request payload.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result containing the response.</returns>
    Task<Result<ScaleResourceResponse>> HandleAsync(ScaleResourceRequest request, CancellationToken cancellationToken);
}
