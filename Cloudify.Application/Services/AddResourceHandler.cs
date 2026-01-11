using Cloudify.Application.Dtos;
using Cloudify.Application.Ports;
using Cloudify.Domain.Models;

namespace Cloudify.Application.Services;

/// <summary>
/// Handles resource creation requests.
/// </summary>
public sealed class AddResourceHandler : IAddResourceHandler
{
    private readonly IStateStore _stateStore;
    private readonly IPortAllocator _portAllocator;
    private readonly IOrchestrator _orchestrator;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddResourceHandler"/> class.
    /// </summary>
    /// <param name="stateStore">The state store.</param>
    /// <param name="portAllocator">The port allocator.</param>
    /// <param name="orchestrator">The orchestrator.</param>
    public AddResourceHandler(IStateStore stateStore, IPortAllocator portAllocator, IOrchestrator orchestrator)
    {
        _stateStore = stateStore ?? throw new ArgumentNullException(nameof(stateStore));
        _portAllocator = portAllocator ?? throw new ArgumentNullException(nameof(portAllocator));
        _orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
    }

    /// <inheritdoc />
    public async Task<Result<AddResourceResponse>> HandleAsync(AddResourceRequest request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return Result<AddResourceResponse>.Fail(ErrorCodes.ValidationFailed, "Request payload is required.");
        }

        if (request.EnvironmentId == Guid.Empty)
        {
            return Result<AddResourceResponse>.Fail(ErrorCodes.ValidationFailed, "Environment identifier is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Result<AddResourceResponse>.Fail(ErrorCodes.ValidationFailed, "Resource name is required.");
        }

        if (!Enum.IsDefined(request.ResourceType))
        {
            return Result<AddResourceResponse>.Fail(ErrorCodes.ValidationFailed, "Resource type is required.");
        }

        if (request.RequestedPort is not null && (request.RequestedPort < 1 || request.RequestedPort > 65535))
        {
            return Result<AddResourceResponse>.Fail(ErrorCodes.ValidationFailed, "Requested port must be between 1 and 65535.");
        }

        if (request.PortPolicy?.ExposedPorts?.Any(port => port < 1 || port > 65535) == true)
        {
            return Result<AddResourceResponse>.Fail(ErrorCodes.ValidationFailed, "Exposed ports must be between 1 and 65535.");
        }

        if (!TryBuildCapacityProfile(request.CapacityProfile, out CapacityProfile? capacityProfile, out string? capacityError))
        {
            return Result<AddResourceResponse>.Fail(ErrorCodes.ValidationFailed, capacityError ?? "Invalid capacity profile.");
        }

        if (!TryBuildStorageProfile(request.ResourceType, request.StorageProfile, out StorageProfile? storageProfile, out string? storageError))
        {
            return Result<AddResourceResponse>.Fail(ErrorCodes.ValidationFailed, storageError ?? "Invalid storage profile.");
        }

        if (!TryBuildCredentialProfile(request.ResourceType, request.CredentialProfile, out CredentialProfile? credentialProfile, out string? credentialError))
        {
            return Result<AddResourceResponse>.Fail(ErrorCodes.ValidationFailed, credentialError ?? "Invalid credential profile.");
        }

        if (request.ResourceType == ResourceType.AppService && string.IsNullOrWhiteSpace(request.Image))
        {
            return Result<AddResourceResponse>.Fail(ErrorCodes.ValidationFailed, "Application service image is required.");
        }

        Environment? environment = await _stateStore.GetEnvironmentAsync(request.EnvironmentId, cancellationToken);
        if (environment is null)
        {
            return Result<AddResourceResponse>.Fail(ErrorCodes.NotFound, "Environment not found.");
        }

        List<int> requestedPorts = request.PortPolicy?.ExposedPorts?.Distinct().ToList() ?? new List<int>();
        PortAllocationResultDto? allocation = null;
        if (requestedPorts.Count > 0 || request.RequestedPort is not null)
        {
            try
            {
                allocation = await _portAllocator.AllocateAsync(request.EnvironmentId, request.ResourceType, request.RequestedPort, cancellationToken);
            }
            catch (PortAllocationException exception)
            {
                return Result<AddResourceResponse>.Fail(ErrorCodes.ValidationFailed, exception.Message);
            }
        }

        List<int> allocatedPorts = BuildPortList(requestedPorts, allocation);
        PortPolicy? portPolicy = allocatedPorts.Count > 0 ? new PortPolicy(allocatedPorts) : null;

        Guid resourceId = Guid.NewGuid();
        DateTimeOffset createdAt = DateTimeOffset.UtcNow;
        Resource resource = CreateResource(request, capacityProfile, storageProfile, credentialProfile, portPolicy, resourceId, createdAt);
        await _stateStore.AddResourceAsync(resource, cancellationToken);

        try
        {
            allocation = await TryAssignPortAsync(
                request,
                requestedPorts,
                allocation,
                capacityProfile,
                storageProfile,
                credentialProfile,
                resourceId,
                createdAt,
                cancellationToken);
        }
        catch (PortAllocationException exception)
        {
            return Result<AddResourceResponse>.Fail(ErrorCodes.Conflict, exception.Message);
        }

        await _orchestrator.DeployEnvironmentAsync(resource.EnvironmentId, cancellationToken);

        ConnectionInfoDto? connectionInfo = BuildConnectionInfo(allocation?.Port, resource);

        return Result<AddResourceResponse>.Ok(new AddResourceResponse
        {
            Resource = MapResource(resource, connectionInfo),
        });
    }

    /// <summary>
    /// Attempts to build a capacity profile from the DTO.
    /// </summary>
    /// <param name="dto">The capacity profile DTO.</param>
    /// <param name="profile">The resulting capacity profile.</param>
    /// <param name="error">The validation error message, if any.</param>
    /// <returns>True when the profile is valid; otherwise, false.</returns>
    private static bool TryBuildCapacityProfile(CapacityProfileDto? dto, out CapacityProfile? profile, out string? error)
    {
        profile = null;
        error = null;

        if (dto is null)
        {
            return true;
        }

        if (dto.CpuLimit is not null && dto.CpuLimit <= 0)
        {
            error = "CPU limit must be greater than zero.";
            return false;
        }

        if (dto.MemoryLimit is not null && dto.MemoryLimit <= 0)
        {
            error = "Memory limit must be greater than zero.";
            return false;
        }

        if (dto.Replicas < 1)
        {
            error = "Replicas must be at least 1.";
            return false;
        }

        if (dto.Notes is not null && string.IsNullOrWhiteSpace(dto.Notes))
        {
            error = "Notes cannot be empty.";
            return false;
        }

        profile = new CapacityProfile(dto.CpuLimit, dto.MemoryLimit, dto.Replicas, dto.Notes);
        return true;
    }

    /// <summary>
    /// Attempts to build a storage profile from the DTO.
    /// </summary>
    /// <param name="resourceType">The resource type.</param>
    /// <param name="dto">The storage profile DTO.</param>
    /// <param name="profile">The resulting storage profile.</param>
    /// <param name="error">The validation error message, if any.</param>
    /// <returns>True when the profile is valid; otherwise, false.</returns>
    private static bool TryBuildStorageProfile(
        ResourceType resourceType,
        StorageProfileDto? dto,
        out StorageProfile? profile,
        out string? error)
    {
        profile = null;
        error = null;

        bool requiresStorage = resourceType is ResourceType.Redis or ResourceType.Postgres or ResourceType.Mongo or ResourceType.Rabbit;
        if (!requiresStorage)
        {
            return true;
        }

        if (dto is null)
        {
            error = "Storage profile is required for the selected resource type.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(dto.VolumeName))
        {
            error = "Storage volume name is required.";
            return false;
        }

        if (dto.SizeGb < 1)
        {
            error = "Storage size must be at least 1 GB.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(dto.MountPath))
        {
            error = "Storage mount path is required.";
            return false;
        }

        profile = new StorageProfile(dto.VolumeName, dto.SizeGb, dto.MountPath, dto.IsPersistent);
        return true;
    }

    /// <summary>
    /// Attempts to build a credential profile from the DTO.
    /// </summary>
    /// <param name="resourceType">The resource type.</param>
    /// <param name="dto">The credential profile DTO.</param>
    /// <param name="profile">The resulting credential profile.</param>
    /// <param name="error">The validation error message, if any.</param>
    /// <returns>True when the profile is valid; otherwise, false.</returns>
    private static bool TryBuildCredentialProfile(
        ResourceType resourceType,
        CredentialProfileDto? dto,
        out CredentialProfile? profile,
        out string? error)
    {
        profile = null;
        error = null;

        bool requiresCredentials = resourceType is ResourceType.Postgres or ResourceType.Mongo or ResourceType.Rabbit;
        if (!requiresCredentials)
        {
            return true;
        }

        if (dto is null)
        {
            error = "Credential profile is required for the selected resource type.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(dto.Username))
        {
            error = "Credential username is required.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(dto.Password))
        {
            error = "Credential password is required.";
            return false;
        }

        profile = new CredentialProfile(dto.Username, dto.Password);
        return true;
    }

    /// <summary>
    /// Builds the effective port list from requested ports and an optional allocation.
    /// </summary>
    /// <param name="requestedPorts">The ports requested in the payload.</param>
    /// <param name="allocation">The allocation result to include.</param>
    /// <returns>The combined list of ports.</returns>
    private static List<int> BuildPortList(IReadOnlyCollection<int> requestedPorts, PortAllocationResultDto? allocation)
    {
        var ports = new List<int>(requestedPorts);
        if (allocation is not null && allocation.Port > 0 && !ports.Contains(allocation.Port))
        {
            ports.Add(allocation.Port);
        }

        return ports;
    }

    /// <summary>
    /// Attempts to assign an allocated port and retries for auto allocations if needed.
    /// </summary>
    /// <param name="request">The original add resource request.</param>
    /// <param name="requestedPorts">The ports requested in the payload.</param>
    /// <param name="allocation">The allocation to assign.</param>
    /// <param name="capacityProfile">The capacity profile.</param>
    /// <param name="storageProfile">The storage profile.</param>
    /// <param name="credentialProfile">The credential profile.</param>
    /// <param name="resourceId">The resource identifier.</param>
    /// <param name="createdAt">The resource creation timestamp.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The final allocation result.</returns>
    /// <exception cref="PortAllocationException">Thrown when a unique port cannot be assigned.</exception>
    private async Task<PortAllocationResultDto?> TryAssignPortAsync(
        AddResourceRequest request,
        IReadOnlyCollection<int> requestedPorts,
        PortAllocationResultDto? allocation,
        CapacityProfile? capacityProfile,
        StorageProfile? storageProfile,
        CredentialProfile? credentialProfile,
        Guid resourceId,
        DateTimeOffset createdAt,
        CancellationToken cancellationToken)
    {
        if (allocation is null || allocation.Port <= 0)
        {
            return allocation;
        }

        const int maxAttempts = 20;
        int attempt = 0;
        PortAllocationResultDto currentAllocation = allocation;

        while (attempt < maxAttempts)
        {
            bool assigned = await _stateStore.AssignPortAsync(
                request.EnvironmentId,
                resourceId,
                currentAllocation.Port,
                cancellationToken);

            if (assigned)
            {
                return currentAllocation;
            }

            if (request.RequestedPort is not null)
            {
                await _stateStore.RemoveResourceAsync(resourceId, cancellationToken);
                throw new PortAllocationException($"Requested port {currentAllocation.Port} is no longer available.");
            }

            attempt++;

            try
            {
                currentAllocation = await _portAllocator.AllocateAsync(
                    request.EnvironmentId,
                    request.ResourceType,
                    null,
                    cancellationToken);
            }
            catch (PortAllocationException)
            {
                await _stateStore.RemoveResourceAsync(resourceId, cancellationToken);
                throw;
            }

            List<int> updatedPorts = BuildPortList(requestedPorts, currentAllocation);
            PortPolicy? updatedPolicy = updatedPorts.Count > 0 ? new PortPolicy(updatedPorts) : null;
            Resource updatedResource = CreateResource(
                request,
                capacityProfile,
                storageProfile,
                credentialProfile,
                updatedPolicy,
                resourceId,
                createdAt);

            await _stateStore.UpdateResourceAsync(updatedResource, cancellationToken);
        }

        await _stateStore.RemoveResourceAsync(resourceId, cancellationToken);
        throw new PortAllocationException("Unable to allocate a unique port for the resource.");
    }

    /// <summary>
    /// Creates a resource instance based on the request.
    /// </summary>
    /// <param name="request">The add resource request.</param>
    /// <param name="capacityProfile">The capacity profile.</param>
    /// <param name="storageProfile">The storage profile.</param>
    /// <param name="credentialProfile">The credential profile.</param>
    /// <param name="portPolicy">The port policy.</param>
    /// <param name="resourceId">The resource identifier.</param>
    /// <param name="createdAt">The resource creation timestamp.</param>
    /// <returns>The created resource.</returns>
    private static Resource CreateResource(
        AddResourceRequest request,
        CapacityProfile? capacityProfile,
        StorageProfile? storageProfile,
        CredentialProfile? credentialProfile,
        PortPolicy? portPolicy,
        Guid resourceId,
        DateTimeOffset createdAt)
    {
        return request.ResourceType switch
        {
            ResourceType.Redis => new RedisResource(
                resourceId,
                request.EnvironmentId,
                request.Name,
                ResourceState.Provisioning,
                createdAt,
                capacityProfile,
                storageProfile!,
                portPolicy),
            ResourceType.Postgres => new PostgresResource(
                resourceId,
                request.EnvironmentId,
                request.Name,
                ResourceState.Provisioning,
                createdAt,
                capacityProfile,
                storageProfile!,
                credentialProfile!,
                portPolicy),
            ResourceType.Mongo => new MongoResource(
                resourceId,
                request.EnvironmentId,
                request.Name,
                ResourceState.Provisioning,
                createdAt,
                capacityProfile,
                storageProfile!,
                credentialProfile!,
                portPolicy),
            ResourceType.Rabbit => new RabbitResource(
                resourceId,
                request.EnvironmentId,
                request.Name,
                ResourceState.Provisioning,
                createdAt,
                capacityProfile,
                storageProfile!,
                credentialProfile!,
                portPolicy),
            ResourceType.AppService => new AppServiceResource(
                resourceId,
                request.EnvironmentId,
                request.Name,
                ResourceState.Provisioning,
                createdAt,
                capacityProfile,
                request.Image!,
                portPolicy),
            _ => throw new InvalidOperationException("Unsupported resource type."),
        };
    }

    /// <summary>
    /// Maps a resource to a summary DTO.
    /// </summary>
    /// <param name="resource">The resource.</param>
    /// <param name="connectionInfo">The connection info.</param>
    /// <returns>The resource summary DTO.</returns>
    private static ResourceSummaryDto MapResource(Resource resource, ConnectionInfoDto? connectionInfo)
    {
        return new ResourceSummaryDto
        {
            Id = resource.Id,
            EnvironmentId = resource.EnvironmentId,
            Name = resource.Name,
            ResourceType = resource.ResourceType,
            State = resource.State,
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
    /// Builds the connection info from the allocated port when available.
    /// </summary>
    /// <param name="allocatedPort">The allocated host port.</param>
    /// <param name="resource">The resource with optional credentials.</param>
    /// <returns>The connection info when a port is provided; otherwise, null.</returns>
    private static ConnectionInfoDto? BuildConnectionInfo(int? allocatedPort, Resource resource)
    {
        if (!allocatedPort.HasValue || allocatedPort.Value <= 0)
        {
            return null;
        }

        var connectionInfo = new ConnectionInfoDto
        {
            Host = "localhost",
            Port = allocatedPort.Value,
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
