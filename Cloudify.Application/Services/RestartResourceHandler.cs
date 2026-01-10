using Cloudify.Application.Dtos;
using Cloudify.Application.Ports;
using Cloudify.Domain.Models;

namespace Cloudify.Application.Services;

/// <summary>
/// Handles resource restart requests.
/// </summary>
public sealed class RestartResourceHandler : IRestartResourceHandler
{
    private readonly IStateStore _stateStore;
    private readonly IOrchestrator _orchestrator;

    /// <summary>
    /// Initializes a new instance of the <see cref="RestartResourceHandler"/> class.
    /// </summary>
    /// <param name="stateStore">The state store.</param>
    /// <param name="orchestrator">The orchestrator.</param>
    public RestartResourceHandler(IStateStore stateStore, IOrchestrator orchestrator)
    {
        _stateStore = stateStore ?? throw new ArgumentNullException(nameof(stateStore));
        _orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
    }

    /// <inheritdoc />
    public async Task<Result<RestartResourceResponse>> HandleAsync(RestartResourceRequest request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return Result<RestartResourceResponse>.Fail(ErrorCodes.ValidationFailed, "Request payload is required.");
        }

        if (request.ResourceId == Guid.Empty)
        {
            return Result<RestartResourceResponse>.Fail(ErrorCodes.ValidationFailed, "Resource identifier is required.");
        }

        Resource? resource = await _stateStore.GetResourceAsync(request.ResourceId, cancellationToken);
        if (resource is null)
        {
            return Result<RestartResourceResponse>.Fail(ErrorCodes.NotFound, "Resource not found.");
        }

        await _orchestrator.RestartResourceAsync(resource.Id, cancellationToken);
        resource.SetState(ResourceState.Running);
        await _stateStore.UpdateResourceAsync(resource, cancellationToken);

        return Result<RestartResourceResponse>.Ok(new RestartResourceResponse
        {
            ResourceId = resource.Id,
            State = resource.State,
        });
    }
}
