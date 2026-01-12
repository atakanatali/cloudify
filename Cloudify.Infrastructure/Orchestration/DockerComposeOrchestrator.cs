using System.Text.Json;
using Cloudify.Application.Ports;
using Cloudify.Domain.Models;
using Cloudify.Infrastructure.Options;
using Cloudify.Infrastructure.Processes;
using Microsoft.Extensions.Options;

namespace Cloudify.Infrastructure.Orchestration;

/// <summary>
/// Provides Docker Compose-based orchestration capabilities.
/// </summary>
public sealed class DockerComposeOrchestrator : IOrchestrator
{
    private readonly ITemplateRenderer _templateRenderer;
    private readonly IStateStore _stateStore;
    private readonly DockerComposeOptions _options;
    private readonly ProcessRunner _processRunner;

    /// <summary>
    /// Initializes a new instance of the <see cref="DockerComposeOrchestrator"/> class.
    /// </summary>
    /// <param name="templateRenderer">The template renderer.</param>
    /// <param name="stateStore">The state store.</param>
    /// <param name="options">The Docker Compose options.</param>
    /// <param name="processRunner">The process runner.</param>
    public DockerComposeOrchestrator(
        ITemplateRenderer templateRenderer,
        IStateStore stateStore,
        IOptions<DockerComposeOptions> options,
        ProcessRunner processRunner)
    {
        _templateRenderer = templateRenderer;
        _stateStore = stateStore;
        _options = options.Value;
        _processRunner = processRunner;
    }

    /// <inheritdoc />
    public async Task DeployEnvironmentAsync(Guid environmentId, CancellationToken cancellationToken)
    {
        string composeYaml = await _templateRenderer.RenderEnvironmentComposeAsync(environmentId, cancellationToken);
        string environmentDirectory = GetEnvironmentDirectory(environmentId);
        string composeFilePath = GetComposeFilePath(environmentId);
        IReadOnlyList<Resource> resources = await _stateStore.ListResourcesAsync(environmentId, cancellationToken);

        Directory.CreateDirectory(environmentDirectory);
        await File.WriteAllTextAsync(composeFilePath, composeYaml, cancellationToken);

        IReadOnlyList<string> commandArguments = BuildDeployArguments(resources);
        (ProcessExecutionResult result, IReadOnlyList<string> arguments) = await RunComposeAsync(
            environmentId,
            commandArguments,
            cancellationToken);

        result.EnsureSuccess(_options.DockerComposeCommand, arguments);
    }

    /// <inheritdoc />
    public async Task StartResourceAsync(Guid resourceId, CancellationToken cancellationToken)
    {
        Resource resource = await GetResourceAsync(resourceId, cancellationToken);
        string serviceName = GetServiceName(resource);

        IReadOnlyList<string> commandArguments = new[] { "start", serviceName };
        (ProcessExecutionResult result, IReadOnlyList<string> arguments) = await RunComposeAsync(
            resource.EnvironmentId,
            commandArguments,
            cancellationToken);

        result.EnsureSuccess(_options.DockerComposeCommand, arguments);
    }

    /// <inheritdoc />
    public async Task StopResourceAsync(Guid resourceId, CancellationToken cancellationToken)
    {
        Resource resource = await GetResourceAsync(resourceId, cancellationToken);
        string serviceName = GetServiceName(resource);

        IReadOnlyList<string> commandArguments = new[] { "stop", serviceName };
        (ProcessExecutionResult result, IReadOnlyList<string> arguments) = await RunComposeAsync(
            resource.EnvironmentId,
            commandArguments,
            cancellationToken);

        result.EnsureSuccess(_options.DockerComposeCommand, arguments);
    }

    /// <inheritdoc />
    public async Task RestartResourceAsync(Guid resourceId, CancellationToken cancellationToken)
    {
        Resource resource = await GetResourceAsync(resourceId, cancellationToken);
        string serviceName = GetServiceName(resource);

        IReadOnlyList<string> commandArguments = new[] { "restart", serviceName };
        (ProcessExecutionResult result, IReadOnlyList<string> arguments) = await RunComposeAsync(
            resource.EnvironmentId,
            commandArguments,
            cancellationToken);

        result.EnsureSuccess(_options.DockerComposeCommand, arguments);
    }

    /// <inheritdoc />
    public async Task ScaleResourceAsync(Guid resourceId, int replicas, CancellationToken cancellationToken)
    {
        Resource resource = await GetResourceAsync(resourceId, cancellationToken);
        string serviceName = GetServiceName(resource);

        IReadOnlyList<string> commandArguments = new[] { "up", "-d", "--scale", $"{serviceName}={replicas}", serviceName };
        (ProcessExecutionResult result, IReadOnlyList<string> arguments) = await RunComposeAsync(
            resource.EnvironmentId,
            commandArguments,
            cancellationToken);

        result.EnsureSuccess(_options.DockerComposeCommand, arguments);
    }

    /// <inheritdoc />
    public async Task<string> GetResourceLogsAsync(Guid resourceId, int tail, string? serviceName, CancellationToken cancellationToken)
    {
        Resource resource = await GetResourceAsync(resourceId, cancellationToken);
        string targetService = string.IsNullOrWhiteSpace(serviceName) ? GetServiceName(resource) : serviceName;

        IReadOnlyList<string> commandArguments = new[] { "logs", "--tail", tail.ToString(), targetService };
        (ProcessExecutionResult result, IReadOnlyList<string> arguments) = await RunComposeAsync(
            resource.EnvironmentId,
            commandArguments,
            cancellationToken);

        result.EnsureSuccess(_options.DockerComposeCommand, arguments);
        return result.StandardOutput.TrimEnd();
    }

    /// <inheritdoc />
    public async Task<ResourceState> GetResourceStatusAsync(Guid resourceId, CancellationToken cancellationToken)
    {
        Resource resource = await GetResourceAsync(resourceId, cancellationToken);
        string serviceName = GetServiceName(resource);

        IReadOnlyList<string> commandArguments = new[] { "ps", "--format", "json" };
        (ProcessExecutionResult result, IReadOnlyList<string> arguments) = await RunComposeAsync(
            resource.EnvironmentId,
            commandArguments,
            cancellationToken);

        result.EnsureSuccess(_options.DockerComposeCommand, arguments);

        return ParseResourceState(result.StandardOutput, serviceName);
    }

    /// <inheritdoc />
    public async Task<ResourceHealth> GetResourceHealthAsync(Guid resourceId, CancellationToken cancellationToken)
    {
        Resource resource = await GetResourceAsync(resourceId, cancellationToken);
        string serviceName = GetServiceName(resource);

        IReadOnlyList<string> commandArguments = new[] { "ps", "--format", "json" };
        (ProcessExecutionResult result, IReadOnlyList<string> arguments) = await RunComposeAsync(
            resource.EnvironmentId,
            commandArguments,
            cancellationToken);

        result.EnsureSuccess(_options.DockerComposeCommand, arguments);

        return ParseResourceHealth(result.StandardOutput, serviceName);
    }

    private async Task<Resource> GetResourceAsync(Guid resourceId, CancellationToken cancellationToken)
    {
        Resource? resource = await _stateStore.GetResourceAsync(resourceId, cancellationToken);

        if (resource is null)
        {
            throw new InvalidOperationException($"Resource '{resourceId}' was not found.");
        }

        return resource;
    }

    private string GetEnvironmentDirectory(Guid environmentId)
    {
        return Path.Combine(_options.WorkingDirectoryBase, environmentId.ToString());
    }

    private string GetComposeFilePath(Guid environmentId)
    {
        return Path.Combine(GetEnvironmentDirectory(environmentId), "docker-compose.yml");
    }

    private string GetProjectName(Guid environmentId)
    {
        return $"cloudify-{environmentId}";
    }

    private string GetServiceName(Resource resource)
    {
        return ComposeNaming.GetServiceName(resource);
    }

    /// <summary>
    /// Builds the compose arguments for deploying an environment with capacity settings.
    /// </summary>
    /// <param name="resources">The resources to deploy.</param>
    /// <returns>The compose argument list.</returns>
    private IReadOnlyList<string> BuildDeployArguments(IReadOnlyList<Resource> resources)
    {
        var args = new List<string> { "up", "-d" };

        foreach (Resource resource in resources)
        {
            if (resource is not AppServiceResource appService)
            {
                continue;
            }

            int replicas = appService.CapacityProfile?.Replicas ?? 1;
            if (replicas <= 1)
            {
                continue;
            }

            args.Add("--scale");
            args.Add($"{GetServiceName(appService)}={replicas}");
        }

        return args;
    }

    private async Task<(ProcessExecutionResult Result, IReadOnlyList<string> Arguments)> RunComposeAsync(
        Guid environmentId,
        IReadOnlyList<string> commandArguments,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<string> arguments = BuildComposeArguments(environmentId, commandArguments);
        var request = new ProcessExecutionRequest
        {
            FileName = _options.DockerComposeCommand,
            Arguments = arguments,
            WorkingDirectory = GetEnvironmentDirectory(environmentId),
            Timeout = _options.CommandTimeoutSeconds > 0
                ? TimeSpan.FromSeconds(_options.CommandTimeoutSeconds)
                : null
        };

        ProcessExecutionResult result = await _processRunner.RunAsync(request, cancellationToken);
        return (result, arguments);
    }

    private IReadOnlyList<string> BuildComposeArguments(Guid environmentId, IReadOnlyList<string> commandArguments)
    {
        string composeFilePath = GetComposeFilePath(environmentId);
        string projectName = GetProjectName(environmentId);

        var args = new List<string>
        {
            _options.ComposeSubcommand,
            "--project-name",
            projectName,
            "--file",
            composeFilePath
        };

        if (_options.EnableDryRun)
        {
            args.Add("--dry-run");
        }

        args.AddRange(commandArguments);

        return args;
    }

    private ResourceState ParseResourceState(string output, string serviceName)
    {
        if (string.IsNullOrWhiteSpace(output))
        {
            return ResourceState.Stopped;
        }

        var services = JsonSerializer.Deserialize<List<DockerComposeServiceStatus>>(output,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (services is null)
        {
            return ResourceState.Failed;
        }

        DockerComposeServiceStatus? service = services.FirstOrDefault(s => string.Equals(s.Service, serviceName, StringComparison.OrdinalIgnoreCase));

        if (service is null)
        {
            return ResourceState.Deleted;
        }

        string state = service.State ?? string.Empty;
        string health = service.Health ?? string.Empty;

        return MapState(state, health);
    }

    /// <summary>
    /// Parses the compose output into a health snapshot for a specific service.
    /// </summary>
    /// <param name="output">The raw compose output.</param>
    /// <param name="serviceName">The service name.</param>
    /// <returns>The parsed resource health snapshot.</returns>
    private ResourceHealth ParseResourceHealth(string output, string serviceName)
    {
        if (string.IsNullOrWhiteSpace(output))
        {
            return new ResourceHealth(ResourceState.Stopped, HealthStatus.Unknown);
        }

        var services = JsonSerializer.Deserialize<List<DockerComposeServiceStatus>>(output,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (services is null)
        {
            return new ResourceHealth(ResourceState.Failed, HealthStatus.Unknown);
        }

        DockerComposeServiceStatus? service = services.FirstOrDefault(s => string.Equals(s.Service, serviceName, StringComparison.OrdinalIgnoreCase));

        if (service is null)
        {
            return new ResourceHealth(ResourceState.Deleted, HealthStatus.Unknown);
        }

        string state = service.State ?? string.Empty;
        string health = service.Health ?? string.Empty;

        ResourceState resourceState = MapState(state, health);
        HealthStatus healthStatus = MapHealth(state, health);
        return new ResourceHealth(resourceState, healthStatus);
    }

    private ResourceState MapState(string state, string health)
    {
        if (state.Contains("running", StringComparison.OrdinalIgnoreCase))
        {
            if (health.Contains("unhealthy", StringComparison.OrdinalIgnoreCase))
            {
                return ResourceState.Failed;
            }

            return ResourceState.Running;
        }

        if (state.Contains("exited", StringComparison.OrdinalIgnoreCase) || state.Contains("stopped", StringComparison.OrdinalIgnoreCase))
        {
            return ResourceState.Stopped;
        }

        if (state.Contains("created", StringComparison.OrdinalIgnoreCase) || state.Contains("restarting", StringComparison.OrdinalIgnoreCase))
        {
            return ResourceState.Provisioning;
        }

        return ResourceState.Failed;
    }

    /// <summary>
    /// Maps compose state and health strings to a unified health status.
    /// </summary>
    /// <param name="state">The container state.</param>
    /// <param name="health">The compose health string.</param>
    /// <returns>The unified health status.</returns>
    private HealthStatus MapHealth(string state, string health)
    {
        if (state.Contains("running", StringComparison.OrdinalIgnoreCase))
        {
            if (health.Contains("unhealthy", StringComparison.OrdinalIgnoreCase))
            {
                return HealthStatus.Unhealthy;
            }

            if (health.Contains("healthy", StringComparison.OrdinalIgnoreCase))
            {
                return HealthStatus.Healthy;
            }

            return HealthStatus.Healthy;
        }

        if (state.Contains("exited", StringComparison.OrdinalIgnoreCase) || state.Contains("stopped", StringComparison.OrdinalIgnoreCase))
        {
            return HealthStatus.Unhealthy;
        }

        if (state.Contains("created", StringComparison.OrdinalIgnoreCase) || state.Contains("restarting", StringComparison.OrdinalIgnoreCase))
        {
            return HealthStatus.Unknown;
        }

        return HealthStatus.Unknown;
    }

    private sealed class DockerComposeServiceStatus
    {
        public string? Service { get; set; }

        public string? State { get; set; }

        public string? Health { get; set; }
    }

}
