using Cloudify.Application.Dtos;
using Cloudify.Application.Ports;
using Cloudify.Domain.Models;
using EnvironmentModel = Cloudify.Domain.Models.Environment;

namespace Cloudify.Application.Services;

/// <summary>
/// Handles environment creation requests.
/// </summary>
public sealed class CreateEnvironmentHandler : ICreateEnvironmentHandler
{
    private readonly IStateStore _stateStore;
    private readonly IOrchestrator _orchestrator;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateEnvironmentHandler"/> class.
    /// </summary>
    /// <param name="stateStore">The state store.</param>
    /// <param name="orchestrator">The orchestrator.</param>
    public CreateEnvironmentHandler(IStateStore stateStore, IOrchestrator orchestrator)
    {
        _stateStore = stateStore ?? throw new ArgumentNullException(nameof(stateStore));
        _orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
    }

    /// <inheritdoc />
    public async Task<Result<CreateEnvironmentResponse>> HandleAsync(CreateEnvironmentRequest request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return Result<CreateEnvironmentResponse>.Fail(ErrorCodes.ValidationFailed, "Request payload is required.");
        }

        if (request.ResourceGroupId == Guid.Empty)
        {
            return Result<CreateEnvironmentResponse>.Fail(ErrorCodes.ValidationFailed, "Resource group identifier is required.");
        }

        if (!Enum.IsDefined(request.Name))
        {
            return Result<CreateEnvironmentResponse>.Fail(ErrorCodes.ValidationFailed, "Environment name is required.");
        }

        if (!Enum.IsDefined(request.NetworkMode))
        {
            return Result<CreateEnvironmentResponse>.Fail(ErrorCodes.ValidationFailed, "Network mode is required.");
        }

        if (request.BaseDomain is not null && string.IsNullOrWhiteSpace(request.BaseDomain))
        {
            return Result<CreateEnvironmentResponse>.Fail(ErrorCodes.ValidationFailed, "Base domain cannot be empty.");
        }

        ResourceGroup? resourceGroup = await _stateStore.GetResourceGroupAsync(request.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
        {
            return Result<CreateEnvironmentResponse>.Fail(ErrorCodes.NotFound, "Resource group not found.");
        }

        var environment = new EnvironmentModel(
            Guid.NewGuid(),
            request.ResourceGroupId,
            request.Name,
            request.NetworkMode,
            request.BaseDomain,
            DateTimeOffset.UtcNow);

        await _stateStore.AddEnvironmentAsync(environment, cancellationToken);
        await _orchestrator.DeployEnvironmentAsync(environment.Id, cancellationToken);

        return Result<CreateEnvironmentResponse>.Ok(new CreateEnvironmentResponse
        {
            Environment = new EnvironmentSummaryDto
            {
                Id = environment.Id,
                ResourceGroupId = environment.ResourceGroupId,
                Name = environment.Name,
                NetworkMode = environment.NetworkMode,
                BaseDomain = environment.BaseDomain,
                CreatedAt = environment.CreatedAt,
            },
        });
    }
}
