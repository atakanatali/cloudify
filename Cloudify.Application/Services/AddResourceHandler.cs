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

    /// <summary>
    /// Initializes a new instance of the <see cref="AddResourceHandler"/> class.
    /// </summary>
    /// <param name="stateStore">The state store.</param>
    /// <param name="portAllocator">The port allocator.</param>
    public AddResourceHandler(IStateStore stateStore, IPortAllocator portAllocator)
    {
        _stateStore = stateStore ?? throw new ArgumentNullException(nameof(stateStore));
        _portAllocator = portAllocator ?? throw new ArgumentNullException(nameof(portAllocator));
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

        var ports = request.PortPolicy?.ExposedPorts?.ToList() ?? new List<int>();
        PortAllocationResultDto? allocation = null;
        if (ports.Count > 0 || request.RequestedPort is not null)
        {
            allocation = await _portAllocator.AllocateAsync(request.EnvironmentId, request.ResourceType, request.RequestedPort, cancellationToken);
            if (allocation.Port > 0 && !ports.Contains(allocation.Port))
            {
                ports.Add(allocation.Port);
            }
        }

        PortPolicy? portPolicy = ports.Count > 0 ? new PortPolicy(ports) : null;

        Resource resource = CreateResource(request, capacityProfile, storageProfile, credentialProfile, portPolicy);
        await _stateStore.AddResourceAsync(resource, cancellationToken);

        if (allocation is not null && allocation.Port > 0)
        {
            await _stateStore.AssignPortAsync(request.EnvironmentId, resource.Id, allocation.Port, cancellationToken);
        }

        ConnectionInfoDto? connectionInfo = BuildConnectionInfo(allocation?.Port);

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
    /// Creates a resource instance based on the request.
    /// </summary>
    /// <param name="request">The add resource request.</param>
    /// <param name="capacityProfile">The capacity profile.</param>
    /// <param name="storageProfile">The storage profile.</param>
    /// <param name="credentialProfile">The credential profile.</param>
    /// <param name="portPolicy">The port policy.</param>
    /// <returns>The created resource.</returns>
    private static Resource CreateResource(
        AddResourceRequest request,
        CapacityProfile? capacityProfile,
        StorageProfile? storageProfile,
        CredentialProfile? credentialProfile,
        PortPolicy? portPolicy)
    {
        DateTimeOffset createdAt = DateTimeOffset.UtcNow;
        return request.ResourceType switch
        {
            ResourceType.Redis => new RedisResource(
                Guid.NewGuid(),
                request.EnvironmentId,
                request.Name,
                ResourceState.Provisioning,
                createdAt,
                capacityProfile,
                storageProfile!,
                portPolicy),
            ResourceType.Postgres => new PostgresResource(
                Guid.NewGuid(),
                request.EnvironmentId,
                request.Name,
                ResourceState.Provisioning,
                createdAt,
                capacityProfile,
                storageProfile!,
                credentialProfile!,
                portPolicy),
            ResourceType.Mongo => new MongoResource(
                Guid.NewGuid(),
                request.EnvironmentId,
                request.Name,
                ResourceState.Provisioning,
                createdAt,
                capacityProfile,
                storageProfile!,
                credentialProfile!,
                portPolicy),
            ResourceType.Rabbit => new RabbitResource(
                Guid.NewGuid(),
                request.EnvironmentId,
                request.Name,
                ResourceState.Provisioning,
                createdAt,
                capacityProfile,
                storageProfile!,
                credentialProfile!,
                portPolicy),
            ResourceType.AppService => new AppServiceResource(
                Guid.NewGuid(),
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
    /// <returns>The connection info when a port is provided; otherwise, null.</returns>
    private static ConnectionInfoDto? BuildConnectionInfo(int? allocatedPort)
    {
        if (!allocatedPort.HasValue || allocatedPort.Value <= 0)
        {
            return null;
        }

        return new ConnectionInfoDto
        {
            Host = "localhost",
            Port = allocatedPort.Value,
        };
    }
}
