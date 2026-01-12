using Cloudify.Application.Dtos;
using Cloudify.Application.Ports;
using Cloudify.Domain.Models;

namespace Cloudify.Application.Services;

/// <summary>
/// Implements environment use cases over the domain model.
/// </summary>
public sealed class EnvironmentService : IEnvironmentService
{
    private readonly IEnvironmentRepository _repository;

    /// <summary>
    /// Initializes a new instance of the <see cref="EnvironmentService"/> class.
    /// </summary>
    /// <param name="repository">The environment repository.</param>
    public EnvironmentService(IEnvironmentRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<CloudEnvironmentDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        IReadOnlyList<CloudEnvironment> environments = await _repository.GetAllAsync(cancellationToken);
        return environments
            .Select(environment => new CloudEnvironmentDto
            {
                Id = environment.Id,
                Name = environment.Name,
                CpuCores = environment.Quota.CpuCores,
                MemoryGb = environment.Quota.MemoryGb,
                StorageGb = environment.Quota.StorageGb,
            })
            .ToArray();
    }

    /// <inheritdoc />
    public async Task CreateAsync(CloudEnvironmentDto request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var quota = new ResourceQuota(request.CpuCores, request.MemoryGb, request.StorageGb);
        var environment = new CloudEnvironment(request.Id, request.Name, quota);
        await _repository.AddAsync(environment, cancellationToken);
    }
}
