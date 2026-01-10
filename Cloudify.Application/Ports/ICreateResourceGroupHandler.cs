using Cloudify.Application.Dtos;

namespace Cloudify.Application.Ports;

/// <summary>
/// Defines the use case for creating a resource group.
/// </summary>
public interface ICreateResourceGroupHandler
{
    /// <summary>
    /// Handles the create resource group request.
    /// </summary>
    /// <param name="request">The request payload.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result containing the response.</returns>
    Task<Result<CreateResourceGroupResponse>> HandleAsync(CreateResourceGroupRequest request, CancellationToken cancellationToken);
}
