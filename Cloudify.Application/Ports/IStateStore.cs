using Cloudify.Domain.Models;
using EnvironmentModel = Cloudify.Domain.Models.Environment;

namespace Cloudify.Application.Ports;

/// <summary>
/// Defines persistence operations for resource groups, environments, and resources.
/// </summary>
public interface IStateStore
{
    /// <summary>
    /// Adds a resource group to the store.
    /// </summary>
    /// <param name="resourceGroup">The resource group.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task AddResourceGroupAsync(ResourceGroup resourceGroup, CancellationToken cancellationToken);

    /// <summary>
    /// Lists all resource groups.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The resource groups.</returns>
    Task<IReadOnlyList<ResourceGroup>> ListResourceGroupsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Gets a resource group by identifier.
    /// </summary>
    /// <param name="resourceGroupId">The resource group identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The resource group or null.</returns>
    Task<ResourceGroup?> GetResourceGroupAsync(Guid resourceGroupId, CancellationToken cancellationToken);

    /// <summary>
    /// Adds an environment to the store.
    /// </summary>
    /// <param name="environment">The environment.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task AddEnvironmentAsync(EnvironmentModel environment, CancellationToken cancellationToken);

    /// <summary>
    /// Gets an environment by identifier.
    /// </summary>
    /// <param name="environmentId">The environment identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The environment or null.</returns>
    Task<EnvironmentModel?> GetEnvironmentAsync(Guid environmentId, CancellationToken cancellationToken);

    /// <summary>
    /// Lists environments for a resource group.
    /// </summary>
    /// <param name="resourceGroupId">The resource group identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The environments.</returns>
    Task<IReadOnlyList<EnvironmentModel>> ListEnvironmentsAsync(Guid resourceGroupId, CancellationToken cancellationToken);

    /// <summary>
    /// Adds a resource to the store.
    /// </summary>
    /// <param name="resource">The resource to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task AddResourceAsync(Resource resource, CancellationToken cancellationToken);

    /// <summary>
    /// Gets a resource by identifier.
    /// </summary>
    /// <param name="resourceId">The resource identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The resource or null.</returns>
    Task<Resource?> GetResourceAsync(Guid resourceId, CancellationToken cancellationToken);

    /// <summary>
    /// Lists resources for an environment.
    /// </summary>
    /// <param name="environmentId">The environment identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The resources.</returns>
    Task<IReadOnlyList<Resource>> ListResourcesAsync(Guid environmentId, CancellationToken cancellationToken);

    /// <summary>
    /// Updates a resource in the store.
    /// </summary>
    /// <param name="resource">The resource to update.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task UpdateResourceAsync(Resource resource, CancellationToken cancellationToken);

    /// <summary>
    /// Removes a resource from the store.
    /// </summary>
    /// <param name="resourceId">The resource identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task RemoveResourceAsync(Guid resourceId, CancellationToken cancellationToken);

    /// <summary>
    /// Assigns a port to a resource.
    /// </summary>
    /// <param name="environmentId">The environment identifier.</param>
    /// <param name="resourceId">The resource identifier.</param>
    /// <param name="port">The allocated port.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True when the port assignment was stored; otherwise, false.</returns>
    Task<bool> AssignPortAsync(Guid environmentId, Guid resourceId, int port, CancellationToken cancellationToken);

    /// <summary>
    /// Lists allocated ports for an environment.
    /// </summary>
    /// <param name="environmentId">The environment identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The allocated ports.</returns>
    Task<IReadOnlyList<int>> ListAllocatedPortsAsync(Guid environmentId, CancellationToken cancellationToken);

    /// <summary>
    /// Lists allocated ports for a resource within an environment.
    /// </summary>
    /// <param name="environmentId">The environment identifier.</param>
    /// <param name="resourceId">The resource identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The allocated ports for the resource.</returns>
    Task<IReadOnlyList<int>> ListResourcePortsAsync(Guid environmentId, Guid resourceId, CancellationToken cancellationToken);

    /// <summary>
    /// Removes ports assigned to a resource.
    /// </summary>
    /// <param name="environmentId">The environment identifier.</param>
    /// <param name="resourceId">The resource identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task RemovePortsAsync(Guid environmentId, Guid resourceId, CancellationToken cancellationToken);
}
