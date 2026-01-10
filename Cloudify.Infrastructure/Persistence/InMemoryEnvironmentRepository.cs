using Cloudify.Application.Ports;
using Cloudify.Domain.Models;

namespace Cloudify.Infrastructure.Persistence;

/// <summary>
/// Provides an in-memory repository for environments.
/// </summary>
public sealed class InMemoryEnvironmentRepository : IEnvironmentRepository
{
    private readonly List<CloudEnvironment> _environments = new();

    /// <inheritdoc />
    public Task<IReadOnlyList<CloudEnvironment>> GetAllAsync(CancellationToken cancellationToken)
    {
        IReadOnlyList<CloudEnvironment> snapshot = _environments.ToArray();
        return Task.FromResult(snapshot);
    }

    /// <inheritdoc />
    public Task AddAsync(CloudEnvironment environment, CancellationToken cancellationToken)
    {
        if (environment is null)
        {
            throw new ArgumentNullException(nameof(environment));
        }

        _environments.Add(environment);
        return Task.CompletedTask;
    }
}
