using Cloudify.Application.Dtos;
using Cloudify.Application.Ports;
using Cloudify.Domain.Models;

namespace Cloudify.Application.Services;

/// <summary>
/// Handles resource health retrieval requests.
/// </summary>
public sealed class GetResourceHealthHandler : IGetResourceHealthHandler
{
    private readonly IStateStore _stateStore;
    private readonly IOrchestrator _orchestrator;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetResourceHealthHandler"/> class.
    /// </summary>
    /// <param name="stateStore">The state store.</param>
    /// <param name="orchestrator">The orchestrator.</param>
    public GetResourceHealthHandler(IStateStore stateStore, IOrchestrator orchestrator)
    {
        _stateStore = stateStore ?? throw new ArgumentNullException(nameof(stateStore));
        _orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
    }

    /// <inheritdoc />
    public async Task<Result<GetResourceHealthResponse>> HandleAsync(GetResourceHealthRequest request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return Result<GetResourceHealthResponse>.Fail(ErrorCodes.ValidationFailed, "Request payload is required.");
        }

        if (request.ResourceId == Guid.Empty)
        {
            return Result<GetResourceHealthResponse>.Fail(ErrorCodes.ValidationFailed, "Resource identifier is required.");
        }

        Resource? resource = await _stateStore.GetResourceAsync(request.ResourceId, cancellationToken);
        if (resource is null)
        {
            return Result<GetResourceHealthResponse>.Fail(ErrorCodes.NotFound, "Resource not found.");
        }

        ResourceState state = await _orchestrator.GetResourceStatusAsync(resource.Id, cancellationToken);
        return Result<GetResourceHealthResponse>.Ok(new GetResourceHealthResponse
        {
            ResourceId = resource.Id,
            State = state,
        });
    }
}
