using Cloudify.Application.Dtos;
using Cloudify.Domain.Models;

namespace Cloudify.Application.Ports;

/// <summary>
/// Defines port allocation operations.
/// </summary>
public interface IPortAllocator
{
    /// <summary>
    /// Allocates a port for a resource.
    /// </summary>
    /// <param name="environmentId">The environment identifier.</param>
    /// <param name="resourceType">The resource type.</param>
    /// <param name="requestedPort">The optional requested port.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The allocated port result.</returns>
    Task<PortAllocationResultDto> AllocateAsync(
        Guid environmentId,
        ResourceType resourceType,
        int? requestedPort,
        CancellationToken cancellationToken);
}
