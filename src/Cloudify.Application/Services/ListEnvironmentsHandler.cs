using Cloudify.Application.Dtos;
using Cloudify.Application.Ports;
using Cloudify.Domain.Models;
using CloudifyDomainEnvironment = Cloudify.Domain.Models.Environment;

namespace Cloudify.Application.Services;

/// <summary>
/// Handles environment list requests.
/// </summary>
public sealed class ListEnvironmentsHandler : IListEnvironmentsHandler
{
    private readonly IStateStore _stateStore;

    /// <summary>
    /// Initializes a new instance of the <see cref="ListEnvironmentsHandler"/> class.
    /// </summary>
    /// <param name="stateStore">The state store.</param>
    public ListEnvironmentsHandler(IStateStore stateStore)
    {
        _stateStore = stateStore ?? throw new ArgumentNullException(nameof(stateStore));
    }

    /// <summary>
    /// Handles the request to list environments for a resource group.
    /// </summary>
    /// <param name="request">The list environments request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The list environments result.</returns>
    public async Task<Result<ListEnvironmentsResponse>> HandleAsync(ListEnvironmentsRequest request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return Result<ListEnvironmentsResponse>.Fail(ErrorCodes.ValidationFailed, "Request payload is required.");
        }

        if (request.ResourceGroupId == Guid.Empty)
        {
            return Result<ListEnvironmentsResponse>.Fail(ErrorCodes.ValidationFailed, "Resource group identifier is required.");
        }

        IReadOnlyList<CloudifyDomainEnvironment> environments = await _stateStore.ListEnvironmentsAsync(request.ResourceGroupId, cancellationToken);

        var response = new ListEnvironmentsResponse
        {
            Environments = environments
                .Select(environment => new EnvironmentSummaryDto
                {
                    Id = environment.Id,
                    ResourceGroupId = environment.ResourceGroupId,
                    Name = environment.Name,
                    NetworkMode = environment.NetworkMode,
                    BaseDomain = environment.BaseDomain,
                    CreatedAt = environment.CreatedAt,
                })
                .ToArray(),
        };

        return Result<ListEnvironmentsResponse>.Ok(response);
    }
}
