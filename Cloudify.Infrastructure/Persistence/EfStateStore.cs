using Cloudify.Application.Ports;
using Cloudify.Domain.Models;
using Cloudify.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using EnvironmentModel = Cloudify.Domain.Models.Environment;

namespace Cloudify.Infrastructure.Persistence;

/// <summary>
/// Provides an Entity Framework Core implementation of <see cref="IStateStore"/>.
/// </summary>
public sealed class EfStateStore : IStateStore
{
    /// <summary>
    /// Stores the database context used to access state records.
    /// </summary>
    private readonly CloudifyDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="EfStateStore"/> class.
    /// </summary>
    /// <param name="dbContext">The database context used for persistence operations.</param>
    public EfStateStore(CloudifyDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    /// <inheritdoc />
    public async Task AddResourceGroupAsync(ResourceGroup resourceGroup, CancellationToken cancellationToken)
    {
        if (resourceGroup is null)
        {
            throw new ArgumentNullException(nameof(resourceGroup));
        }

        var record = new ResourceGroupRecord
        {
            Id = resourceGroup.Id,
            Name = resourceGroup.Name,
            CreatedAt = resourceGroup.CreatedAt,
            Tags = MapTagRecords(resourceGroup.Tags, resourceGroup.Id),
        };

        _dbContext.ResourceGroups.Add(record);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ResourceGroup>> ListResourceGroupsAsync(CancellationToken cancellationToken)
    {
        List<ResourceGroupRecord> records = await _dbContext.ResourceGroups
            .AsNoTracking()
            .Include(record => record.Tags)
            .OrderBy(record => record.CreatedAt)
            .ToListAsync(cancellationToken);

        return records.Select(MapResourceGroup).ToArray();
    }

    /// <inheritdoc />
    public async Task<ResourceGroup?> GetResourceGroupAsync(Guid resourceGroupId, CancellationToken cancellationToken)
    {
        ResourceGroupRecord? record = await _dbContext.ResourceGroups
            .AsNoTracking()
            .Include(group => group.Tags)
            .FirstOrDefaultAsync(group => group.Id == resourceGroupId, cancellationToken);

        return record is null ? null : MapResourceGroup(record);
    }

    /// <inheritdoc />
    public async Task AddEnvironmentAsync(EnvironmentModel environment, CancellationToken cancellationToken)
    {
        if (environment is null)
        {
            throw new ArgumentNullException(nameof(environment));
        }

        var record = new EnvironmentRecord
        {
            Id = environment.Id,
            ResourceGroupId = environment.ResourceGroupId,
            Name = environment.Name,
            NetworkMode = environment.NetworkMode,
            BaseDomain = environment.BaseDomain,
            CreatedAt = environment.CreatedAt,
        };

        _dbContext.Environments.Add(record);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<EnvironmentModel?> GetEnvironmentAsync(Guid environmentId, CancellationToken cancellationToken)
    {
        EnvironmentRecord? record = await _dbContext.Environments
            .AsNoTracking()
            .FirstOrDefaultAsync(environment => environment.Id == environmentId, cancellationToken);

        return record is null ? null : MapEnvironment(record);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<EnvironmentModel>> ListEnvironmentsAsync(Guid resourceGroupId, CancellationToken cancellationToken)
    {
        List<EnvironmentRecord> records = await _dbContext.Environments
            .AsNoTracking()
            .Where(environment => environment.ResourceGroupId == resourceGroupId)
            .OrderBy(environment => environment.CreatedAt)
            .ToListAsync(cancellationToken);

        return records.Select(MapEnvironment).ToArray();
    }

    /// <inheritdoc />
    public async Task AddResourceAsync(Resource resource, CancellationToken cancellationToken)
    {
        if (resource is null)
        {
            throw new ArgumentNullException(nameof(resource));
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        ResourceRecord record = CreateResourceRecord(resource);
        _dbContext.Resources.Add(record);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Resource?> GetResourceAsync(Guid resourceId, CancellationToken cancellationToken)
    {
        ResourceRecord? record = await _dbContext.Resources
            .AsNoTracking()
            .Include(resource => resource.CapacityProfile)
            .Include(resource => resource.StorageProfile)
            .Include(resource => resource.PortPolicies)
            .FirstOrDefaultAsync(resource => resource.Id == resourceId, cancellationToken);

        return record is null ? null : MapResource(record);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Resource>> ListResourcesAsync(Guid environmentId, CancellationToken cancellationToken)
    {
        List<ResourceRecord> records = await _dbContext.Resources
            .AsNoTracking()
            .Where(resource => resource.EnvironmentId == environmentId)
            .Include(resource => resource.CapacityProfile)
            .Include(resource => resource.StorageProfile)
            .Include(resource => resource.PortPolicies)
            .OrderBy(resource => resource.CreatedAt)
            .ToListAsync(cancellationToken);

        return records.Select(MapResource).ToArray();
    }

    /// <inheritdoc />
    public async Task UpdateResourceAsync(Resource resource, CancellationToken cancellationToken)
    {
        if (resource is null)
        {
            throw new ArgumentNullException(nameof(resource));
        }

        ResourceRecord? record = await _dbContext.Resources
            .Include(existing => existing.CapacityProfile)
            .Include(existing => existing.StorageProfile)
            .Include(existing => existing.PortPolicies)
            .FirstOrDefaultAsync(existing => existing.Id == resource.Id, cancellationToken);

        if (record is null)
        {
            return;
        }

        record.Name = resource.Name;
        record.EnvironmentId = resource.EnvironmentId;
        record.ResourceType = resource.ResourceType;
        record.State = resource.State;
        record.CreatedAt = resource.CreatedAt;

        ApplyCapacityProfile(record, resource);
        ApplyStorageProfile(record, resource);
        ApplyPortPolicies(record, resource);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task RemoveResourceAsync(Guid resourceId, CancellationToken cancellationToken)
    {
        ResourceRecord? record = await _dbContext.Resources
            .FirstOrDefaultAsync(resource => resource.Id == resourceId, cancellationToken);

        if (record is null)
        {
            return;
        }

        _dbContext.Resources.Remove(record);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task AssignPortAsync(Guid environmentId, Guid resourceId, int port, CancellationToken cancellationToken)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        bool exists = await _dbContext.ResourcePorts
            .AnyAsync(allocation => allocation.EnvironmentId == environmentId && allocation.Port == port, cancellationToken);

        if (exists)
        {
            return;
        }

        var record = new ResourcePortRecord
        {
            EnvironmentId = environmentId,
            ResourceId = resourceId,
            Port = port,
        };

        _dbContext.ResourcePorts.Add(record);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<int>> ListAllocatedPortsAsync(Guid environmentId, CancellationToken cancellationToken)
    {
        List<int> ports = await _dbContext.ResourcePorts
            .AsNoTracking()
            .Where(port => port.EnvironmentId == environmentId)
            .OrderBy(port => port.Port)
            .Select(port => port.Port)
            .ToListAsync(cancellationToken);

        return ports;
    }

    /// <inheritdoc />
    public async Task RemovePortsAsync(Guid environmentId, Guid resourceId, CancellationToken cancellationToken)
    {
        List<ResourcePortRecord> records = await _dbContext.ResourcePorts
            .Where(port => port.EnvironmentId == environmentId && port.ResourceId == resourceId)
            .ToListAsync(cancellationToken);

        if (records.Count == 0)
        {
            return;
        }

        _dbContext.ResourcePorts.RemoveRange(records);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Maps a resource group record to the domain model.
    /// </summary>
    /// <param name="record">The resource group record.</param>
    /// <returns>The resource group domain model.</returns>
    private static ResourceGroup MapResourceGroup(ResourceGroupRecord record)
    {
        Dictionary<string, string> tags = record.Tags
            .ToDictionary(tag => tag.Key, tag => tag.Value, StringComparer.OrdinalIgnoreCase);

        return new ResourceGroup(record.Id, record.Name, record.CreatedAt, tags);
    }

    /// <summary>
    /// Maps an environment record to the domain model.
    /// </summary>
    /// <param name="record">The environment record.</param>
    /// <returns>The environment domain model.</returns>
    private static EnvironmentModel MapEnvironment(EnvironmentRecord record)
    {
        return new EnvironmentModel(
            record.Id,
            record.ResourceGroupId,
            record.Name,
            record.NetworkMode,
            record.BaseDomain,
            record.CreatedAt);
    }

    /// <summary>
    /// Maps a resource record to the domain model.
    /// </summary>
    /// <param name="record">The resource record.</param>
    /// <returns>The resource domain model.</returns>
    private static Resource MapResource(ResourceRecord record)
    {
        CapacityProfile? capacityProfile = record.CapacityProfile is null
            ? null
            : new CapacityProfile(
                record.CapacityProfile.CpuLimit,
                record.CapacityProfile.MemoryLimit,
                record.CapacityProfile.Replicas,
                record.CapacityProfile.Notes);

        PortPolicy? portPolicy = record.PortPolicies.Count == 0
            ? null
            : new PortPolicy(record.PortPolicies.Select(policy => policy.Port));

        StorageProfile? storageProfile = record.StorageProfile is null
            ? null
            : new StorageProfile(
                record.StorageProfile.VolumeName,
                record.StorageProfile.SizeGb,
                record.StorageProfile.MountPath,
                record.StorageProfile.IsPersistent);

        return record.ResourceType switch
        {
            ResourceType.Redis => new RedisResource(
                record.Id,
                record.EnvironmentId,
                record.Name,
                record.State,
                record.CreatedAt,
                capacityProfile,
                storageProfile ?? throw new InvalidOperationException("Storage profile is required for Redis resources."),
                portPolicy),
            ResourceType.Postgres => new PostgresResource(
                record.Id,
                record.EnvironmentId,
                record.Name,
                record.State,
                record.CreatedAt,
                capacityProfile,
                storageProfile ?? throw new InvalidOperationException("Storage profile is required for PostgreSQL resources."),
                portPolicy),
            ResourceType.Mongo => new MongoResource(
                record.Id,
                record.EnvironmentId,
                record.Name,
                record.State,
                record.CreatedAt,
                capacityProfile,
                storageProfile ?? throw new InvalidOperationException("Storage profile is required for MongoDB resources."),
                portPolicy),
            ResourceType.Rabbit => new RabbitResource(
                record.Id,
                record.EnvironmentId,
                record.Name,
                record.State,
                record.CreatedAt,
                capacityProfile,
                storageProfile ?? throw new InvalidOperationException("Storage profile is required for RabbitMQ resources."),
                portPolicy),
            ResourceType.AppService => new AppServiceResource(
                record.Id,
                record.EnvironmentId,
                record.Name,
                record.State,
                record.CreatedAt,
                capacityProfile,
                portPolicy),
            _ => throw new InvalidOperationException("Unsupported resource type.")
        };
    }

    /// <summary>
    /// Maps tag values to resource group tag records.
    /// </summary>
    /// <param name="tags">The tag dictionary.</param>
    /// <param name="resourceGroupId">The owning resource group identifier.</param>
    /// <returns>The list of tag records.</returns>
    private static List<ResourceGroupTagRecord> MapTagRecords(IReadOnlyDictionary<string, string> tags, Guid resourceGroupId)
    {
        return tags.Select(tag => new ResourceGroupTagRecord
        {
            ResourceGroupId = resourceGroupId,
            Key = tag.Key,
            Value = tag.Value,
        }).ToList();
    }

    /// <summary>
    /// Creates a resource record from the domain model.
    /// </summary>
    /// <param name="resource">The resource domain model.</param>
    /// <returns>The resource record ready for persistence.</returns>
    private static ResourceRecord CreateResourceRecord(Resource resource)
    {
        ResourceRecord record = resource.ResourceType switch
        {
            ResourceType.Redis => new RedisResourceRecord(),
            ResourceType.Postgres => new PostgresResourceRecord(),
            ResourceType.Mongo => new MongoResourceRecord(),
            ResourceType.Rabbit => new RabbitResourceRecord(),
            ResourceType.AppService => new AppServiceResourceRecord(),
            _ => throw new InvalidOperationException("Unsupported resource type."),
        };

        record.Id = resource.Id;
        record.EnvironmentId = resource.EnvironmentId;
        record.Name = resource.Name;
        record.ResourceType = resource.ResourceType;
        record.State = resource.State;
        record.CreatedAt = resource.CreatedAt;
        record.CapacityProfile = MapCapacityProfileRecord(resource, resource.Id);
        record.StorageProfile = MapStorageProfileRecord(resource, resource.Id);
        record.PortPolicies = MapPortPolicyRecords(resource, resource.Id);

        return record;
    }

    /// <summary>
    /// Maps the capacity profile on a resource to a persistence record.
    /// </summary>
    /// <param name="resource">The resource domain model.</param>
    /// <param name="resourceId">The owning resource identifier.</param>
    /// <returns>The capacity profile record or null when not present.</returns>
    private static CapacityProfileRecord? MapCapacityProfileRecord(Resource resource, Guid resourceId)
    {
        if (resource.CapacityProfile is null)
        {
            return null;
        }

        return new CapacityProfileRecord
        {
            ResourceId = resourceId,
            CpuLimit = resource.CapacityProfile.CpuLimit,
            MemoryLimit = resource.CapacityProfile.MemoryLimit,
            Replicas = resource.CapacityProfile.Replicas,
            Notes = resource.CapacityProfile.Notes,
        };
    }

    /// <summary>
    /// Maps the storage profile on a resource to a persistence record.
    /// </summary>
    /// <param name="resource">The resource domain model.</param>
    /// <param name="resourceId">The owning resource identifier.</param>
    /// <returns>The storage profile record or null when not present.</returns>
    private static StorageProfileRecord? MapStorageProfileRecord(Resource resource, Guid resourceId)
    {
        StorageProfile? profile = resource switch
        {
            RedisResource redis => redis.StorageProfile,
            PostgresResource postgres => postgres.StorageProfile,
            MongoResource mongo => mongo.StorageProfile,
            RabbitResource rabbit => rabbit.StorageProfile,
            _ => null,
        };

        if (profile is null)
        {
            return null;
        }

        return new StorageProfileRecord
        {
            ResourceId = resourceId,
            VolumeName = profile.VolumeName,
            SizeGb = profile.SizeGb,
            MountPath = profile.MountPath,
            IsPersistent = profile.IsPersistent,
        };
    }

    /// <summary>
    /// Maps the port policy on a resource to persistence records.
    /// </summary>
    /// <param name="resource">The resource domain model.</param>
    /// <param name="resourceId">The owning resource identifier.</param>
    /// <returns>The list of port policy records.</returns>
    private static List<ResourcePortPolicyRecord> MapPortPolicyRecords(Resource resource, Guid resourceId)
    {
        if (resource.PortPolicy is null)
        {
            return new List<ResourcePortPolicyRecord>();
        }

        return resource.PortPolicy.ExposedPorts.Select(port => new ResourcePortPolicyRecord
        {
            ResourceId = resourceId,
            Port = port,
        }).ToList();
    }

    /// <summary>
    /// Applies the capacity profile changes from the domain model to the record.
    /// </summary>
    /// <param name="record">The resource record to update.</param>
    /// <param name="resource">The resource domain model.</param>
    private void ApplyCapacityProfile(ResourceRecord record, Resource resource)
    {
        if (resource.CapacityProfile is null)
        {
            if (record.CapacityProfile is not null)
            {
                _dbContext.CapacityProfiles.Remove(record.CapacityProfile);
                record.CapacityProfile = null;
            }

            return;
        }

        if (record.CapacityProfile is null)
        {
            record.CapacityProfile = new CapacityProfileRecord
            {
                ResourceId = record.Id,
                CpuLimit = resource.CapacityProfile.CpuLimit,
                MemoryLimit = resource.CapacityProfile.MemoryLimit,
                Replicas = resource.CapacityProfile.Replicas,
                Notes = resource.CapacityProfile.Notes,
            };
            return;
        }

        record.CapacityProfile.CpuLimit = resource.CapacityProfile.CpuLimit;
        record.CapacityProfile.MemoryLimit = resource.CapacityProfile.MemoryLimit;
        record.CapacityProfile.Replicas = resource.CapacityProfile.Replicas;
        record.CapacityProfile.Notes = resource.CapacityProfile.Notes;
    }

    /// <summary>
    /// Applies the storage profile changes from the domain model to the record.
    /// </summary>
    /// <param name="record">The resource record to update.</param>
    /// <param name="resource">The resource domain model.</param>
    private void ApplyStorageProfile(ResourceRecord record, Resource resource)
    {
        StorageProfile? profile = resource switch
        {
            RedisResource redis => redis.StorageProfile,
            PostgresResource postgres => postgres.StorageProfile,
            MongoResource mongo => mongo.StorageProfile,
            RabbitResource rabbit => rabbit.StorageProfile,
            _ => null,
        };

        if (profile is null)
        {
            if (record.StorageProfile is not null)
            {
                _dbContext.StorageProfiles.Remove(record.StorageProfile);
                record.StorageProfile = null;
            }

            return;
        }

        if (record.StorageProfile is null)
        {
            record.StorageProfile = new StorageProfileRecord
            {
                ResourceId = record.Id,
                VolumeName = profile.VolumeName,
                SizeGb = profile.SizeGb,
                MountPath = profile.MountPath,
                IsPersistent = profile.IsPersistent,
            };
            return;
        }

        record.StorageProfile.VolumeName = profile.VolumeName;
        record.StorageProfile.SizeGb = profile.SizeGb;
        record.StorageProfile.MountPath = profile.MountPath;
        record.StorageProfile.IsPersistent = profile.IsPersistent;
    }

    /// <summary>
    /// Applies the port policy changes from the domain model to the record.
    /// </summary>
    /// <param name="record">The resource record to update.</param>
    /// <param name="resource">The resource domain model.</param>
    private void ApplyPortPolicies(ResourceRecord record, Resource resource)
    {
        if (record.PortPolicies.Count > 0)
        {
            _dbContext.ResourcePortPolicies.RemoveRange(record.PortPolicies);
            record.PortPolicies.Clear();
        }

        if (resource.PortPolicy is null)
        {
            return;
        }

        foreach (int port in resource.PortPolicy.ExposedPorts)
        {
            record.PortPolicies.Add(new ResourcePortPolicyRecord
            {
                ResourceId = record.Id,
                Port = port,
            });
        }
    }
}
