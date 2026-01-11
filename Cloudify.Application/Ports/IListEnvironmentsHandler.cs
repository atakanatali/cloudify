using Cloudify.Application.Dtos;

namespace Cloudify.Application.Ports;

/// <summary>
/// Defines a handler for listing environments by resource group.
/// </summary>
public interface IListEnvironmentsHandler
{
    /// <summary>
    /// Handles the list environments request.
    /// </summary>
    /// <param name="request">The list environments request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The list environments result.</returns>
    Task<Result<ListEnvironmentsResponse>> HandleAsync(ListEnvironmentsRequest request, CancellationToken cancellationToken);
}
