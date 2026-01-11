using Cloudify.Application.Dtos;
using Cloudify.Application.Ports;
using Cloudify.Domain.Models;
using EnvironmentModel = Cloudify.Domain.Models.Environment;

namespace Cloudify.Application.Services;

/// <summary>
/// Handles environment overview retrieval.
/// </summary>
public sealed class GetEnvironmentOverviewHandler : IGetEnvironmentOverviewHandler
{
    private readonly IStateStore _stateStore;
    private readonly ITemplateRenderer _templateRenderer;
    private readonly ISystemProfileProvider _systemProfileProvider;
    private readonly IOrchestrator _orchestrator;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetEnvironmentOverviewHandler"/> class.
    /// </summary>
    /// <param name="stateStore">The state store.</param>
    /// <param name="templateRenderer">The template renderer.</param>
    /// <param name="systemProfileProvider">The system profile provider.</param>
    /// <param name="orchestrator">The orchestrator.</param>
    public GetEnvironmentOverviewHandler(
        IStateStore stateStore,
        ITemplateRenderer templateRenderer,
        ISystemProfileProvider systemProfileProvider,
        IOrchestrator orchestrator)
    {
        _stateStore = stateStore ?? throw new ArgumentNullException(nameof(stateStore));
        _templateRenderer = templateRenderer ?? throw new ArgumentNullException(nameof(templateRenderer));
        _systemProfileProvider = systemProfileProvider ?? throw new ArgumentNullException(nameof(systemProfileProvider));
        _orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
    }

    /// <inheritdoc />
    public async Task<Result<GetEnvironmentOverviewResponse>> HandleAsync(GetEnvironmentOverviewRequest request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return Result<GetEnvironmentOverviewResponse>.Fail(ErrorCodes.ValidationFailed, "Request payload is required.");
        }

        if (request.EnvironmentId == Guid.Empty)
        {
            return Result<GetEnvironmentOverviewResponse>.Fail(ErrorCodes.ValidationFailed, "Environment identifier is required.");
        }

        EnvironmentModel? environment = await _stateStore.GetEnvironmentAsync(request.EnvironmentId, cancellationToken);
        if (environment is null)
        {
            return Result<GetEnvironmentOverviewResponse>.Fail(ErrorCodes.NotFound, "Environment not found.");
        }

        IReadOnlyList<Resource> resources = await _stateStore.ListResourcesAsync(environment.Id, cancellationToken);
        string composeYaml = await _templateRenderer.RenderEnvironmentComposeAsync(environment.Id, cancellationToken);
        HostProfileDto hostProfile = await _systemProfileProvider.GetHostProfileAsync(cancellationToken);

        var connectionInfoLookup = new Dictionary<Guid, ConnectionInfoDto?>();
        foreach (Resource resource in resources)
        {
            IReadOnlyList<int> ports = await _stateStore.ListResourcePortsAsync(environment.Id, resource.Id, cancellationToken);
            connectionInfoLookup[resource.Id] = BuildConnectionInfo(resource, ports);
        }

        var healthStatuses = new Dictionary<Guid, HealthStatus>();
        foreach (Resource resource in resources)
        {
            healthStatuses[resource.Id] = await GetHealthStatusAsync(resource, cancellationToken);
        }

        var overview = new EnvironmentOverviewDto
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
            Resources = resources.Select(resource =>
                MapResource(
                    resource,
                    connectionInfoLookup.TryGetValue(resource.Id, out ConnectionInfoDto? info) ? info : null,
                    healthStatuses.TryGetValue(resource.Id, out HealthStatus status) ? status : HealthStatus.Unknown)).ToArray(),
            ComposeYaml = composeYaml,
            HostProfile = hostProfile,
        };

        return Result<GetEnvironmentOverviewResponse>.Ok(new GetEnvironmentOverviewResponse
        {
            Overview = overview,
        });
    }

    /// <summary>
    /// Maps a resource to a summary DTO.
    /// </summary>
    /// <param name="resource">The resource.</param>
    /// <param name="connectionInfo">The connection info.</param>
    /// <returns>The resource summary DTO.</returns>
    private static ResourceSummaryDto MapResource(Resource resource, ConnectionInfoDto? connectionInfo, HealthStatus healthStatus)
    {
        return new ResourceSummaryDto
        {
            Id = resource.Id,
            EnvironmentId = resource.EnvironmentId,
            Name = resource.Name,
            ResourceType = resource.ResourceType,
            State = resource.State,
            HealthStatus = healthStatus,
            CreatedAt = resource.CreatedAt,
            CapacityProfile = resource.CapacityProfile is null
                ? null
                : new CapacityProfileDto
                {
                    CpuLimit = resource.CapacityProfile.CpuLimit,
                    MemoryLimit = resource.CapacityProfile.MemoryLimit,
                    Replicas = resource.CapacityProfile.Replicas,
                    Notes = resource.CapacityProfile.Notes,
                },
            StorageProfile = MapStorageProfile(resource),
            PortPolicy = resource.PortPolicy is null
                ? null
                : new PortPolicyDto
                {
                    ExposedPorts = resource.PortPolicy.ExposedPorts.ToArray(),
                },
            ConnectionInfo = connectionInfo,
        };
    }

    /// <summary>
    /// Retrieves the health status for a resource with graceful fallback.
    /// </summary>
    /// <param name="resource">The resource.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The health status.</returns>
    private async Task<HealthStatus> GetHealthStatusAsync(Resource resource, CancellationToken cancellationToken)
    {
        try
        {
            ResourceHealth health = await _orchestrator.GetResourceHealthAsync(resource.Id, cancellationToken);
            return health.Status;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            return HealthStatus.Unknown;
        }
    }

    /// <summary>
    /// Maps the storage profile to a DTO when applicable.
    /// </summary>
    /// <param name="resource">The resource.</param>
    /// <returns>The storage profile DTO or null.</returns>
    private static StorageProfileDto? MapStorageProfile(Resource resource)
    {
        return resource switch
        {
            RedisResource redis => new StorageProfileDto
            {
                VolumeName = redis.StorageProfile.VolumeName,
                SizeGb = redis.StorageProfile.SizeGb,
                MountPath = redis.StorageProfile.MountPath,
                IsPersistent = redis.StorageProfile.IsPersistent,
            },
            PostgresResource postgres => new StorageProfileDto
            {
                VolumeName = postgres.StorageProfile.VolumeName,
                SizeGb = postgres.StorageProfile.SizeGb,
                MountPath = postgres.StorageProfile.MountPath,
                IsPersistent = postgres.StorageProfile.IsPersistent,
            },
            MongoResource mongo => new StorageProfileDto
            {
                VolumeName = mongo.StorageProfile.VolumeName,
                SizeGb = mongo.StorageProfile.SizeGb,
                MountPath = mongo.StorageProfile.MountPath,
                IsPersistent = mongo.StorageProfile.IsPersistent,
            },
            RabbitResource rabbit => new StorageProfileDto
            {
                VolumeName = rabbit.StorageProfile.VolumeName,
                SizeGb = rabbit.StorageProfile.SizeGb,
                MountPath = rabbit.StorageProfile.MountPath,
                IsPersistent = rabbit.StorageProfile.IsPersistent,
            },
            _ => null,
        };
    }

    /// <summary>
    /// Builds the connection info for the resource from the allocated ports.
    /// </summary>
    /// <param name="resource">The resource.</param>
    /// <param name="allocatedPorts">The allocated host ports.</param>
    /// <returns>The connection info when ports are available; otherwise, null.</returns>
    private static ConnectionInfoDto? BuildConnectionInfo(Resource resource, IReadOnlyList<int> allocatedPorts)
    {
        if (allocatedPorts.Count == 0)
        {
            return null;
        }

        int port = allocatedPorts.OrderBy(port => port).First();
        var connectionInfo = new ConnectionInfoDto
        {
            Host = "localhost",
            Port = port,
        };

        switch (resource)
        {
            case PostgresResource postgres:
                connectionInfo.Username = postgres.CredentialProfile.Username;
                connectionInfo.Password = postgres.CredentialProfile.Password;
                break;
            case MongoResource mongo:
                connectionInfo.Username = mongo.CredentialProfile.Username;
                connectionInfo.Password = mongo.CredentialProfile.Password;
                break;
            case RabbitResource rabbit:
                connectionInfo.Username = rabbit.CredentialProfile.Username;
                connectionInfo.Password = rabbit.CredentialProfile.Password;
                break;
        }

        return connectionInfo;
    }
}
