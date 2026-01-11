using Cloudify.Application.Dtos;
using Cloudify.Application.Ports;
using Cloudify.Domain.Models;
using System.Net.Http;

namespace Cloudify.Application.Services;

/// <summary>
/// Handles resource health retrieval requests.
/// </summary>
public sealed class GetResourceHealthHandler : IGetResourceHealthHandler
{
    private readonly IStateStore _stateStore;
    private readonly IOrchestrator _orchestrator;
    private readonly HttpClient _httpClient = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetResourceHealthHandler"/> class.
    /// </summary>
    /// <param name="stateStore">The state store.</param>
    /// <param name="orchestrator">The orchestrator.</param>
    public GetResourceHealthHandler(IStateStore stateStore, IOrchestrator orchestrator)
    {
        _stateStore = stateStore ?? throw new ArgumentNullException(nameof(stateStore));
        _orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
    }

    /// <inheritdoc />
    public async Task<Result<GetResourceHealthResponse>> HandleAsync(GetResourceHealthRequest request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return Result<GetResourceHealthResponse>.Fail(ErrorCodes.ValidationFailed, "Request payload is required.");
        }

        if (request.ResourceId == Guid.Empty)
        {
            return Result<GetResourceHealthResponse>.Fail(ErrorCodes.ValidationFailed, "Resource identifier is required.");
        }

        Resource? resource = await _stateStore.GetResourceAsync(request.ResourceId, cancellationToken);
        if (resource is null)
        {
            return Result<GetResourceHealthResponse>.Fail(ErrorCodes.NotFound, "Resource not found.");
        }

        ResourceHealth health = await _orchestrator.GetResourceHealthAsync(resource.Id, cancellationToken);
        HealthStatus status = await ApplyAppServiceHealthCheckAsync(resource, health.Status, cancellationToken);
        return Result<GetResourceHealthResponse>.Ok(new GetResourceHealthResponse
        {
            ResourceId = resource.Id,
            State = health.State,
            HealthStatus = status,
        });
    }

    /// <summary>
    /// Applies an optional application service health check when configured.
    /// </summary>
    /// <param name="resource">The resource to check.</param>
    /// <param name="baseStatus">The base health status.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated health status.</returns>
    private async Task<HealthStatus> ApplyAppServiceHealthCheckAsync(
        Resource resource,
        HealthStatus baseStatus,
        CancellationToken cancellationToken)
    {
        if (resource is not AppServiceResource appService)
        {
            return baseStatus;
        }

        if (string.IsNullOrWhiteSpace(appService.HealthEndpointPath))
        {
            return baseStatus;
        }

        if (baseStatus != HealthStatus.Healthy)
        {
            return baseStatus;
        }

        IReadOnlyList<int> ports = await _stateStore.ListResourcePortsAsync(resource.EnvironmentId, resource.Id, cancellationToken);
        if (ports.Count == 0)
        {
            return HealthStatus.Unknown;
        }

        int port = ports.OrderBy(port => port).First();
        string address = $"http://localhost:{port}{appService.HealthEndpointPath}";
        try
        {
            using HttpRequestMessage request = new(HttpMethod.Get, address);
            using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);
            return response.IsSuccessStatusCode ? HealthStatus.Healthy : HealthStatus.Unhealthy;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (HttpRequestException)
        {
            return HealthStatus.Unhealthy;
        }
    }
}
