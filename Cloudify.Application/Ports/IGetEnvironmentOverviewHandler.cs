using Cloudify.Application.Dtos;

namespace Cloudify.Application.Ports;

/// <summary>
/// Defines the use case for retrieving environment overview data.
/// </summary>
public interface IGetEnvironmentOverviewHandler
{
    /// <summary>
    /// Handles the get environment overview request.
    /// </summary>
    /// <param name="request">The request payload.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result containing the response.</returns>
    Task<Result<GetEnvironmentOverviewResponse>> HandleAsync(GetEnvironmentOverviewRequest request, CancellationToken cancellationToken);
}
