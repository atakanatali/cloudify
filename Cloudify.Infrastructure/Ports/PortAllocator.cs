using System.Net;
using System.Net.Sockets;
using Cloudify.Application.Dtos;
using Cloudify.Application.Ports;
using Cloudify.Domain.Models;

namespace Cloudify.Infrastructure.Ports;

/// <summary>
/// Provides deterministic port allocation for Cloudify resources.
/// </summary>
public sealed class PortAllocator : IPortAllocator
{
    /// <summary>
    /// Defines the minimum valid TCP port.
    /// </summary>
    private const int MinPort = 1;

    /// <summary>
    /// Defines the maximum valid TCP port.
    /// </summary>
    private const int MaxPort = 65535;

    /// <summary>
    /// Defines base ports for supported resource types.
    /// </summary>
    private static readonly IReadOnlyDictionary<ResourceType, int[]> BasePorts = new Dictionary<ResourceType, int[]>
    {
        [ResourceType.Redis] = new[] { 6379 },
        [ResourceType.Postgres] = new[] { 5432 },
        [ResourceType.Mongo] = new[] { 27017 },
        [ResourceType.Rabbit] = new[] { 5672, 15672 },
        [ResourceType.AppService] = new[] { 8080, 5000 },
    };

    /// <summary>
    /// The state store used to read allocated ports.
    /// </summary>
    private readonly IStateStore _stateStore;

    /// <summary>
    /// Initializes a new instance of the <see cref="PortAllocator"/> class.
    /// </summary>
    /// <param name="stateStore">The state store.</param>
    public PortAllocator(IStateStore stateStore)
    {
        _stateStore = stateStore ?? throw new ArgumentNullException(nameof(stateStore));
    }

    /// <inheritdoc />
    public async Task<PortAllocationResultDto> AllocateAsync(
        Guid environmentId,
        ResourceType resourceType,
        int? requestedPort,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<int> allocatedPorts = await _stateStore.ListAllocatedPortsAsync(environmentId, cancellationToken);
        var reservedPorts = new HashSet<int>(allocatedPorts);

        if (requestedPort is not null)
        {
            return AllocateRequestedPort(reservedPorts, requestedPort.Value);
        }

        return AllocateAutomaticPort(reservedPorts, resourceType);
    }

    /// <summary>
    /// Allocates a requested port after validating availability.
    /// </summary>
    /// <param name="allocatedPorts">The allocated ports for the environment.</param>
    /// <param name="requestedPort">The requested port.</param>
    /// <returns>The allocation result.</returns>
    private static PortAllocationResultDto AllocateRequestedPort(IReadOnlySet<int> allocatedPorts, int requestedPort)
    {
        if (requestedPort is < MinPort or > MaxPort)
        {
            throw new PortAllocationException("Requested port must be between 1 and 65535.");
        }

        if (allocatedPorts.Contains(requestedPort))
        {
            throw new PortAllocationException($"Requested port {requestedPort} is already allocated in this environment.");
        }

        if (!IsPortAvailable(requestedPort))
        {
            throw new PortAllocationException($"Requested port {requestedPort} is already bound on the host.");
        }

        return new PortAllocationResultDto
        {
            Port = requestedPort,
            IsRequestedPort = true,
        };
    }

    /// <summary>
    /// Allocates the next available port based on the resource base port strategy.
    /// </summary>
    /// <param name="allocatedPorts">The allocated ports for the environment.</param>
    /// <param name="resourceType">The resource type.</param>
    /// <returns>The allocation result.</returns>
    private static PortAllocationResultDto AllocateAutomaticPort(IReadOnlySet<int> allocatedPorts, ResourceType resourceType)
    {
        if (!BasePorts.TryGetValue(resourceType, out int[]? basePorts))
        {
            throw new PortAllocationException("Resource type does not support automatic port allocation.");
        }

        for (int offset = 0; offset <= MaxPort; offset++)
        {
            foreach (int basePort in basePorts)
            {
                int candidate = basePort + offset;
                if (candidate > MaxPort)
                {
                    continue;
                }

                if (allocatedPorts.Contains(candidate))
                {
                    continue;
                }

                if (!IsPortAvailable(candidate))
                {
                    continue;
                }

                return new PortAllocationResultDto
                {
                    Port = candidate,
                    IsRequestedPort = false,
                };
            }
        }

        throw new PortAllocationException("No available ports could be allocated for the requested resource.");
    }

    /// <summary>
    /// Checks whether the specified TCP port can be bound on localhost.
    /// </summary>
    /// <param name="port">The TCP port to test.</param>
    /// <returns>True when the port can be bound on localhost; otherwise, false.</returns>
    private static bool IsPortAvailable(int port)
    {
        try
        {
            var listener = new TcpListener(IPAddress.Loopback, port);
            listener.Start();
            listener.Stop();
            return true;
        }
        catch (SocketException)
        {
            return false;
        }
        catch (ArgumentOutOfRangeException)
        {
            return false;
        }
    }
}
