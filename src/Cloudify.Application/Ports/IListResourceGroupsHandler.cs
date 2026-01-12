using Cloudify.Application.Dtos;

namespace Cloudify.Application.Ports;

/// <summary>
/// Defines the use case for listing resource groups.
/// </summary>
public interface IListResourceGroupsHandler
{
    /// <summary>
    /// Handles the list resource groups request.
    /// </summary>
    /// <param name="request">The request payload.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result containing the response.</returns>
    Task<Result<ListResourceGroupsResponse>> HandleAsync(ListResourceGroupsRequest request, CancellationToken cancellationToken);
}
