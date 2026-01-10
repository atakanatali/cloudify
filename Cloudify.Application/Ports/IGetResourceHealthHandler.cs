using Cloudify.Application.Dtos;

namespace Cloudify.Application.Ports;

/// <summary>
/// Defines the use case for retrieving resource health.
/// </summary>
public interface IGetResourceHealthHandler
{
    /// <summary>
    /// Handles the get resource health request.
    /// </summary>
    /// <param name="request">The request payload.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result containing the response.</returns>
    Task<Result<GetResourceHealthResponse>> HandleAsync(GetResourceHealthRequest request, CancellationToken cancellationToken);
}
