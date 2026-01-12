using Cloudify.Application.Dtos;

namespace Cloudify.Application.Ports;

/// <summary>
/// Defines the use case for retrieving resource logs.
/// </summary>
public interface IGetResourceLogsHandler
{
    /// <summary>
    /// Handles the get resource logs request.
    /// </summary>
    /// <param name="request">The request payload.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result containing the response.</returns>
    Task<Result<GetResourceLogsResponse>> HandleAsync(GetResourceLogsRequest request, CancellationToken cancellationToken);
}
