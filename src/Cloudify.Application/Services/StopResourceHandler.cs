using Cloudify.Application.Dtos;
using Cloudify.Application.Ports;
using Cloudify.Domain.Models;

namespace Cloudify.Application.Services;

/// <summary>
/// Handles resource stop requests.
/// </summary>
public sealed class StopResourceHandler : IStopResourceHandler
{
    private readonly IStateStore _stateStore;
    private readonly IOrchestrator _orchestrator;

    /// <summary>
    /// Initializes a new instance of the <see cref="StopResourceHandler"/> class.
    /// </summary>
    /// <param name="stateStore">The state store.</param>
    /// <param name="orchestrator">The orchestrator.</param>
    public StopResourceHandler(IStateStore stateStore, IOrchestrator orchestrator)
    {
        _stateStore = stateStore ?? throw new ArgumentNullException(nameof(stateStore));
        _orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
    }

    /// <inheritdoc />
    public async Task<Result<StopResourceResponse>> HandleAsync(StopResourceRequest request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return Result<StopResourceResponse>.Fail(ErrorCodes.ValidationFailed, "Request payload is required.");
        }

        if (request.ResourceId == Guid.Empty)
        {
            return Result<StopResourceResponse>.Fail(ErrorCodes.ValidationFailed, "Resource identifier is required.");
        }

        Resource? resource = await _stateStore.GetResourceAsync(request.ResourceId, cancellationToken);
        if (resource is null)
        {
            return Result<StopResourceResponse>.Fail(ErrorCodes.NotFound, "Resource not found.");
        }

        await _orchestrator.StopResourceAsync(resource.Id, cancellationToken);
        resource.SetState(ResourceState.Stopped);
        await _stateStore.UpdateResourceAsync(resource, cancellationToken);

        return Result<StopResourceResponse>.Ok(new StopResourceResponse
        {
            ResourceId = resource.Id,
            State = resource.State,
        });
    }
}
