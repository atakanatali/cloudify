using Cloudify.Application.Dtos;
using Cloudify.Application.Ports;

namespace Cloudify.Application.Services;

/// <summary>
/// Handles resource deletion requests.
/// </summary>
public sealed class DeleteResourceHandler : IDeleteResourceHandler
{
    private readonly IStateStore _stateStore;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteResourceHandler"/> class.
    /// </summary>
    /// <param name="stateStore">The state store.</param>
    public DeleteResourceHandler(IStateStore stateStore)
    {
        _stateStore = stateStore ?? throw new ArgumentNullException(nameof(stateStore));
    }

    /// <inheritdoc />
    public async Task<Result<DeleteResourceResponse>> HandleAsync(DeleteResourceRequest request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return Result<DeleteResourceResponse>.Fail(ErrorCodes.ValidationFailed, "Request payload is required.");
        }

        if (request.ResourceId == Guid.Empty)
        {
            return Result<DeleteResourceResponse>.Fail(ErrorCodes.ValidationFailed, "Resource identifier is required.");
        }

        var resource = await _stateStore.GetResourceAsync(request.ResourceId, cancellationToken);
        if (resource is null)
        {
            return Result<DeleteResourceResponse>.Fail(ErrorCodes.NotFound, "Resource not found.");
        }

        await _stateStore.RemovePortsAsync(resource.EnvironmentId, resource.Id, cancellationToken);
        await _stateStore.RemoveResourceAsync(resource.Id, cancellationToken);

        return Result<DeleteResourceResponse>.Ok(new DeleteResourceResponse
        {
            ResourceId = resource.Id,
        });
    }
}
