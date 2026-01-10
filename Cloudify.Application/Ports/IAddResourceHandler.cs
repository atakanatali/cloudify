using Cloudify.Application.Dtos;

namespace Cloudify.Application.Ports;

/// <summary>
/// Defines the use case for adding a resource.
/// </summary>
public interface IAddResourceHandler
{
    /// <summary>
    /// Handles the add resource request.
    /// </summary>
    /// <param name="request">The request payload.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result containing the response.</returns>
    Task<Result<AddResourceResponse>> HandleAsync(AddResourceRequest request, CancellationToken cancellationToken);
}
