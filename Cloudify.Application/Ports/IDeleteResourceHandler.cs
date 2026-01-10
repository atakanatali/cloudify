using Cloudify.Application.Dtos;

namespace Cloudify.Application.Ports;

/// <summary>
/// Defines the use case for deleting a resource.
/// </summary>
public interface IDeleteResourceHandler
{
    /// <summary>
    /// Handles the delete resource request.
    /// </summary>
    /// <param name="request">The request payload.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result containing the response.</returns>
    Task<Result<DeleteResourceResponse>> HandleAsync(DeleteResourceRequest request, CancellationToken cancellationToken);
}
