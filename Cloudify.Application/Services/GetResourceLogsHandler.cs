using Cloudify.Application.Dtos;
using Cloudify.Application.Ports;

namespace Cloudify.Application.Services;

/// <summary>
/// Handles resource log retrieval requests.
/// </summary>
public sealed class GetResourceLogsHandler : IGetResourceLogsHandler
{
    private readonly IStateStore _stateStore;
    private readonly IOrchestrator _orchestrator;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetResourceLogsHandler"/> class.
    /// </summary>
    /// <param name="stateStore">The state store.</param>
    /// <param name="orchestrator">The orchestrator.</param>
    public GetResourceLogsHandler(IStateStore stateStore, IOrchestrator orchestrator)
    {
        _stateStore = stateStore ?? throw new ArgumentNullException(nameof(stateStore));
        _orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
    }

    /// <inheritdoc />
    public async Task<Result<GetResourceLogsResponse>> HandleAsync(GetResourceLogsRequest request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return Result<GetResourceLogsResponse>.Fail(ErrorCodes.ValidationFailed, "Request payload is required.");
        }

        if (request.ResourceId == Guid.Empty)
        {
            return Result<GetResourceLogsResponse>.Fail(ErrorCodes.ValidationFailed, "Resource identifier is required.");
        }

        if (request.Tail < 1)
        {
            return Result<GetResourceLogsResponse>.Fail(ErrorCodes.ValidationFailed, "Tail must be at least 1.");
        }

        var resource = await _stateStore.GetResourceAsync(request.ResourceId, cancellationToken);
        if (resource is null)
        {
            return Result<GetResourceLogsResponse>.Fail(ErrorCodes.NotFound, "Resource not found.");
        }

        string? serviceName = string.IsNullOrWhiteSpace(request.ServiceName) ? null : request.ServiceName.Trim();
        string logs = await _orchestrator.GetResourceLogsAsync(request.ResourceId, request.Tail, serviceName, cancellationToken);
        return Result<GetResourceLogsResponse>.Ok(new GetResourceLogsResponse
        {
            ResourceId = request.ResourceId,
            Logs = logs,
        });
    }
}
