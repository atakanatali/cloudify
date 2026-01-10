using Cloudify.Application.Dtos;

namespace Cloudify.Application.Ports;

/// <summary>
/// Defines the use case for starting a resource.
/// </summary>
public interface IStartResourceHandler
{
    /// <summary>
    /// Handles the start resource request.
    /// </summary>
    /// <param name="request">The request payload.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result containing the response.</returns>
    Task<Result<StartResourceResponse>> HandleAsync(StartResourceRequest request, CancellationToken cancellationToken);
}
