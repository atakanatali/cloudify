using Cloudify.Application.Dtos;
using Cloudify.Application.Ports;
using Cloudify.Domain.Models;

namespace Cloudify.Application.Services;

/// <summary>
/// Handles resource scaling requests.
/// </summary>
public sealed class ScaleResourceHandler : IScaleResourceHandler
{
    private readonly IStateStore _stateStore;
    private readonly IOrchestrator _orchestrator;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScaleResourceHandler"/> class.
    /// </summary>
    /// <param name="stateStore">The state store.</param>
    /// <param name="orchestrator">The orchestrator.</param>
    public ScaleResourceHandler(IStateStore stateStore, IOrchestrator orchestrator)
    {
        _stateStore = stateStore ?? throw new ArgumentNullException(nameof(stateStore));
        _orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
    }

    /// <inheritdoc />
    public async Task<Result<ScaleResourceResponse>> HandleAsync(ScaleResourceRequest request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return Result<ScaleResourceResponse>.Fail(ErrorCodes.ValidationFailed, "Request payload is required.");
        }

        if (request.ResourceId == Guid.Empty)
        {
            return Result<ScaleResourceResponse>.Fail(ErrorCodes.ValidationFailed, "Resource identifier is required.");
        }

        if (request.Replicas < 1)
        {
            return Result<ScaleResourceResponse>.Fail(ErrorCodes.ValidationFailed, "Replicas must be at least 1.");
        }

        Resource? resource = await _stateStore.GetResourceAsync(request.ResourceId, cancellationToken);
        if (resource is null)
        {
            return Result<ScaleResourceResponse>.Fail(ErrorCodes.NotFound, "Resource not found.");
        }

        CapacityProfile capacityProfile = resource.CapacityProfile is null
            ? new CapacityProfile(null, null, request.Replicas, null)
            : new CapacityProfile(resource.CapacityProfile.CpuLimit, resource.CapacityProfile.MemoryLimit, request.Replicas, resource.CapacityProfile.Notes);

        Resource updatedResource = CloneResource(resource, capacityProfile);
        await _orchestrator.ScaleResourceAsync(resource.Id, request.Replicas, cancellationToken);
        await _stateStore.UpdateResourceAsync(updatedResource, cancellationToken);

        return Result<ScaleResourceResponse>.Ok(new ScaleResourceResponse
        {
            ResourceId = resource.Id,
            Replicas = request.Replicas,
        });
    }

    /// <summary>
    /// Creates a new resource instance with the updated capacity profile.
    /// </summary>
    /// <param name="resource">The source resource.</param>
    /// <param name="capacityProfile">The updated capacity profile.</param>
    /// <returns>The cloned resource.</returns>
    private static Resource CloneResource(Resource resource, CapacityProfile capacityProfile)
    {
        return resource switch
        {
            RedisResource redis => new RedisResource(
                redis.Id,
                redis.EnvironmentId,
                redis.Name,
                redis.State,
                redis.CreatedAt,
                capacityProfile,
                redis.StorageProfile,
                redis.PortPolicy),
            PostgresResource postgres => new PostgresResource(
                postgres.Id,
                postgres.EnvironmentId,
                postgres.Name,
                postgres.State,
                postgres.CreatedAt,
                capacityProfile,
                postgres.StorageProfile,
                postgres.CredentialProfile,
                postgres.PortPolicy),
            MongoResource mongo => new MongoResource(
                mongo.Id,
                mongo.EnvironmentId,
                mongo.Name,
                mongo.State,
                mongo.CreatedAt,
                capacityProfile,
                mongo.StorageProfile,
                mongo.CredentialProfile,
                mongo.PortPolicy),
            RabbitResource rabbit => new RabbitResource(
                rabbit.Id,
                rabbit.EnvironmentId,
                rabbit.Name,
                rabbit.State,
                rabbit.CreatedAt,
                capacityProfile,
                rabbit.StorageProfile,
                rabbit.CredentialProfile,
                rabbit.PortPolicy),
            AppServiceResource app => new AppServiceResource(
                app.Id,
                app.EnvironmentId,
                app.Name,
                app.State,
                app.CreatedAt,
                capacityProfile,
                app.Image,
                app.PortPolicy,
                app.HealthEndpointPath),
            _ => throw new InvalidOperationException("Unsupported resource type."),
        };
    }
}
