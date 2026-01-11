using System.Text;
using Cloudify.Application.Ports;
using Cloudify.Domain.Models;

namespace Cloudify.Infrastructure.Orchestration;

/// <summary>
/// Renders deterministic Docker Compose templates for Cloudify environments.
/// </summary>
public sealed class DockerComposeTemplateRenderer : ITemplateRenderer
{
    private const string ComposeVersion = "3.9";
    private const string HostAddress = "localhost";

    private readonly IStateStore _stateStore;

    /// <summary>
    /// Initializes a new instance of the <see cref="DockerComposeTemplateRenderer"/> class.
    /// </summary>
    /// <param name="stateStore">The state store.</param>
    public DockerComposeTemplateRenderer(IStateStore stateStore)
    {
        _stateStore = stateStore ?? throw new ArgumentNullException(nameof(stateStore));
    }

    /// <inheritdoc />
    public async Task<string> RenderEnvironmentComposeAsync(Guid environmentId, CancellationToken cancellationToken)
    {
        IReadOnlyList<Resource> resources = await _stateStore.ListResourcesAsync(environmentId, cancellationToken);
        var services = new List<ComposeServiceDefinition>();
        var volumes = new SortedDictionary<string, ComposeVolumeDefinition>(StringComparer.Ordinal);

        foreach (Resource resource in resources)
        {
            IReadOnlyList<int> hostPorts = await _stateStore.ListResourcePortsAsync(environmentId, resource.Id, cancellationToken);
            ComposeServiceDefinition service = BuildServiceDefinition(environmentId, resource, hostPorts, volumes);
            services.Add(service);
        }

        List<ComposeServiceDefinition> orderedServices = services
            .OrderBy(service => service.Name, StringComparer.Ordinal)
            .ToList();

        return RenderComposeYaml(orderedServices, volumes.Values);
    }

    /// <summary>
    /// Builds the Compose service definition for a resource.
    /// </summary>
    /// <param name="environmentId">The environment identifier.</param>
    /// <param name="resource">The resource to render.</param>
    /// <param name="hostPorts">The allocated host ports.</param>
    /// <param name="volumes">The volume registry to populate.</param>
    /// <returns>The Compose service definition.</returns>
    private static ComposeServiceDefinition BuildServiceDefinition(
        Guid environmentId,
        Resource resource,
        IReadOnlyList<int> hostPorts,
        SortedDictionary<string, ComposeVolumeDefinition> volumes)
    {
        string serviceName = ComposeNaming.GetServiceName(resource);
        string image = ResolveImage(resource);
        IReadOnlyList<int> containerPorts = ResolveContainerPorts(resource);
        IReadOnlyList<ComposePortMapping> ports = BuildPortMappings(hostPorts, containerPorts);
        IReadOnlyDictionary<string, string> environment = BuildEnvironmentVariables(resource);
        IReadOnlyList<ComposeVolumeMount> volumeMounts = BuildVolumeMounts(environmentId, resource, volumes);
        ComposeHealthcheckDefinition? healthcheck = BuildHealthcheck(resource);

        return new ComposeServiceDefinition(
            serviceName,
            image,
            ports,
            environment,
            volumeMounts,
            healthcheck);
    }

    /// <summary>
    /// Resolves the container image for the resource.
    /// </summary>
    /// <param name="resource">The resource to resolve.</param>
    /// <returns>The container image name.</returns>
    private static string ResolveImage(Resource resource)
    {
        return resource switch
        {
            RedisResource => "redis:7.2.4",
            PostgresResource => "postgres:16.4",
            MongoResource => "mongo:7.0.12",
            RabbitResource => "rabbitmq:3.12.14-management",
            AppServiceResource appService => appService.Image,
            _ => "busybox:latest",
        };
    }

    /// <summary>
    /// Resolves the container ports for the resource.
    /// </summary>
    /// <param name="resource">The resource to resolve.</param>
    /// <returns>The ordered list of container ports.</returns>
    private static IReadOnlyList<int> ResolveContainerPorts(Resource resource)
    {
        if (resource.PortPolicy is not null)
        {
            return resource.PortPolicy.ExposedPorts.OrderBy(port => port).ToArray();
        }

        return resource.ResourceType switch
        {
            ResourceType.Redis => new[] { 6379 },
            ResourceType.Postgres => new[] { 5432 },
            ResourceType.Mongo => new[] { 27017 },
            ResourceType.Rabbit => new[] { 5672, 15672 },
            _ => Array.Empty<int>(),
        };
    }

    /// <summary>
    /// Builds port mappings from allocated host ports to container ports.
    /// </summary>
    /// <param name="hostPorts">The allocated host ports.</param>
    /// <param name="containerPorts">The target container ports.</param>
    /// <returns>The mapped ports for Compose.</returns>
    private static IReadOnlyList<ComposePortMapping> BuildPortMappings(
        IReadOnlyList<int> hostPorts,
        IReadOnlyList<int> containerPorts)
    {
        if (hostPorts.Count == 0 || containerPorts.Count == 0)
        {
            return Array.Empty<ComposePortMapping>();
        }

        int count = Math.Min(hostPorts.Count, containerPorts.Count);
        var orderedHosts = hostPorts.OrderBy(port => port).ToArray();
        var mappings = new List<ComposePortMapping>(count);

        for (int index = 0; index < count; index++)
        {
            mappings.Add(new ComposePortMapping(orderedHosts[index], containerPorts[index]));
        }

        return mappings;
    }

    /// <summary>
    /// Builds environment variables for the resource.
    /// </summary>
    /// <param name="resource">The resource to configure.</param>
    /// <returns>The environment variable map.</returns>
    private static IReadOnlyDictionary<string, string> BuildEnvironmentVariables(Resource resource)
    {
        return resource switch
        {
            PostgresResource postgres => new SortedDictionary<string, string>(StringComparer.Ordinal)
            {
                ["POSTGRES_USER"] = postgres.CredentialProfile.Username,
                ["POSTGRES_PASSWORD"] = postgres.CredentialProfile.Password,
            },
            MongoResource mongo => new SortedDictionary<string, string>(StringComparer.Ordinal)
            {
                ["MONGO_INITDB_ROOT_USERNAME"] = mongo.CredentialProfile.Username,
                ["MONGO_INITDB_ROOT_PASSWORD"] = mongo.CredentialProfile.Password,
            },
            RabbitResource rabbit => new SortedDictionary<string, string>(StringComparer.Ordinal)
            {
                ["RABBITMQ_DEFAULT_USER"] = rabbit.CredentialProfile.Username,
                ["RABBITMQ_DEFAULT_PASS"] = rabbit.CredentialProfile.Password,
            },
            _ => new SortedDictionary<string, string>(StringComparer.Ordinal),
        };
    }

    /// <summary>
    /// Builds volume mount entries for the resource.
    /// </summary>
    /// <param name="environmentId">The environment identifier.</param>
    /// <param name="resource">The resource to configure.</param>
    /// <param name="volumes">The volume registry to populate.</param>
    /// <returns>The volume mount list.</returns>
    private static IReadOnlyList<ComposeVolumeMount> BuildVolumeMounts(
        Guid environmentId,
        Resource resource,
        SortedDictionary<string, ComposeVolumeDefinition> volumes)
    {
        StorageProfile? storageProfile = resource switch
        {
            RedisResource redis => redis.StorageProfile,
            PostgresResource postgres => postgres.StorageProfile,
            MongoResource mongo => mongo.StorageProfile,
            RabbitResource rabbit => rabbit.StorageProfile,
            _ => null,
        };

        if (storageProfile is null)
        {
            return Array.Empty<ComposeVolumeMount>();
        }

        string volumeName = ComposeNaming.GetVolumeName(environmentId, resource.Id);
        if (!volumes.ContainsKey(volumeName))
        {
            volumes[volumeName] = new ComposeVolumeDefinition(volumeName);
        }

        return new[] { new ComposeVolumeMount(volumeName, storageProfile.MountPath) };
    }

    /// <summary>
    /// Builds a healthcheck definition for the resource.
    /// </summary>
    /// <param name="resource">The resource to monitor.</param>
    /// <returns>The healthcheck definition or null when not applicable.</returns>
    private static ComposeHealthcheckDefinition? BuildHealthcheck(Resource resource)
    {
        return resource switch
        {
            PostgresResource postgres => new ComposeHealthcheckDefinition(
                new[] { "CMD-SHELL", $"pg_isready -U {postgres.CredentialProfile.Username}" },
                "10s",
                "5s",
                5),
            MongoResource mongo => new ComposeHealthcheckDefinition(
                new[]
                {
                    "CMD-SHELL",
                    $"mongosh --username \"{mongo.CredentialProfile.Username}\" --password \"{mongo.CredentialProfile.Password}\" --eval \"db.adminCommand('ping')\""
                },
                "10s",
                "5s",
                5),
            RabbitResource => new ComposeHealthcheckDefinition(
                new[] { "CMD", "rabbitmq-diagnostics", "ping" },
                "10s",
                "5s",
                5),
            RedisResource => new ComposeHealthcheckDefinition(
                new[] { "CMD", "redis-cli", "ping" },
                "10s",
                "5s",
                5),
            _ => null,
        };
    }

    /// <summary>
    /// Renders the Compose YAML text for the given services and volumes.
    /// </summary>
    /// <param name="services">The ordered list of services.</param>
    /// <param name="volumes">The ordered list of volumes.</param>
    /// <returns>The compose YAML document.</returns>
    private static string RenderComposeYaml(
        IReadOnlyList<ComposeServiceDefinition> services,
        IEnumerable<ComposeVolumeDefinition> volumes)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"version: \"{ComposeVersion}\"");
        builder.AppendLine("services:");

        foreach (ComposeServiceDefinition service in services)
        {
            builder.AppendLine($"  {service.Name}:");
            builder.AppendLine($"    image: \"{service.Image}\"");

            if (service.Ports.Count > 0)
            {
                builder.AppendLine("    ports:");
                foreach (ComposePortMapping port in service.Ports)
                {
                    builder.AppendLine($"      - \"{HostAddress}:{port.HostPort}:{port.ContainerPort}\"");
                }
            }

            if (service.Environment.Count > 0)
            {
                builder.AppendLine("    environment:");
                foreach (KeyValuePair<string, string> variable in service.Environment.OrderBy(pair => pair.Key, StringComparer.Ordinal))
                {
                    builder.AppendLine($"      {variable.Key}: {QuoteYaml(variable.Value)}");
                }
            }

            if (service.Volumes.Count > 0)
            {
                builder.AppendLine("    volumes:");
                foreach (ComposeVolumeMount volume in service.Volumes)
                {
                    builder.AppendLine($"      - \"{volume.Name}:{volume.MountPath}\"");
                }
            }

            if (service.Healthcheck is not null)
            {
                builder.AppendLine("    healthcheck:");
                builder.AppendLine("      test:");
                foreach (string testSegment in service.Healthcheck.Test)
                {
                    builder.AppendLine($"        - {QuoteYaml(testSegment)}");
                }
                builder.AppendLine($"      interval: \"{service.Healthcheck.Interval}\"");
                builder.AppendLine($"      timeout: \"{service.Healthcheck.Timeout}\"");
                builder.AppendLine($"      retries: {service.Healthcheck.Retries}");
            }
        }

        if (volumes.Any())
        {
            builder.AppendLine("volumes:");
            foreach (ComposeVolumeDefinition volume in volumes.OrderBy(volume => volume.Name, StringComparer.Ordinal))
            {
                builder.AppendLine($"  {volume.Name}:");
                builder.AppendLine($"    name: \"{volume.Name}\"");
            }
        }

        return builder.ToString();
    }

    /// <summary>
    /// Quotes a YAML scalar using single quotes.
    /// </summary>
    /// <param name="value">The value to quote.</param>
    /// <returns>The quoted YAML scalar.</returns>
    private static string QuoteYaml(string value)
    {
        string escaped = value.Replace("'", "''", StringComparison.Ordinal);
        return $"'{escaped}'";
    }

    /// <summary>
    /// Represents a Compose service definition.
    /// </summary>
    private sealed class ComposeServiceDefinition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ComposeServiceDefinition"/> class.
        /// </summary>
        /// <param name="name">The service name.</param>
        /// <param name="image">The container image.</param>
        /// <param name="ports">The port mappings.</param>
        /// <param name="environment">The environment variables.</param>
        /// <param name="volumes">The volume mounts.</param>
        /// <param name="healthcheck">The healthcheck definition.</param>
        public ComposeServiceDefinition(
            string name,
            string image,
            IReadOnlyList<ComposePortMapping> ports,
            IReadOnlyDictionary<string, string> environment,
            IReadOnlyList<ComposeVolumeMount> volumes,
            ComposeHealthcheckDefinition? healthcheck)
        {
            Name = name;
            Image = image;
            Ports = ports;
            Environment = environment;
            Volumes = volumes;
            Healthcheck = healthcheck;
        }

        /// <summary>
        /// Gets the service name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the container image.
        /// </summary>
        public string Image { get; }

        /// <summary>
        /// Gets the port mappings.
        /// </summary>
        public IReadOnlyList<ComposePortMapping> Ports { get; }

        /// <summary>
        /// Gets the environment variable map.
        /// </summary>
        public IReadOnlyDictionary<string, string> Environment { get; }

        /// <summary>
        /// Gets the volume mounts.
        /// </summary>
        public IReadOnlyList<ComposeVolumeMount> Volumes { get; }

        /// <summary>
        /// Gets the healthcheck definition.
        /// </summary>
        public ComposeHealthcheckDefinition? Healthcheck { get; }
    }

    /// <summary>
    /// Represents a Compose volume definition.
    /// </summary>
    private sealed class ComposeVolumeDefinition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ComposeVolumeDefinition"/> class.
        /// </summary>
        /// <param name="name">The volume name.</param>
        public ComposeVolumeDefinition(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Gets the volume name.
        /// </summary>
        public string Name { get; }
    }

    /// <summary>
    /// Represents a Compose volume mount.
    /// </summary>
    private sealed class ComposeVolumeMount
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ComposeVolumeMount"/> class.
        /// </summary>
        /// <param name="name">The volume name.</param>
        /// <param name="mountPath">The mount path.</param>
        public ComposeVolumeMount(string name, string mountPath)
        {
            Name = name;
            MountPath = mountPath;
        }

        /// <summary>
        /// Gets the volume name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the mount path.
        /// </summary>
        public string MountPath { get; }
    }

    /// <summary>
    /// Represents a Compose port mapping.
    /// </summary>
    private sealed class ComposePortMapping
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ComposePortMapping"/> class.
        /// </summary>
        /// <param name="hostPort">The host port.</param>
        /// <param name="containerPort">The container port.</param>
        public ComposePortMapping(int hostPort, int containerPort)
        {
            HostPort = hostPort;
            ContainerPort = containerPort;
        }

        /// <summary>
        /// Gets the host port.
        /// </summary>
        public int HostPort { get; }

        /// <summary>
        /// Gets the container port.
        /// </summary>
        public int ContainerPort { get; }
    }

    /// <summary>
    /// Represents a Compose healthcheck definition.
    /// </summary>
    private sealed class ComposeHealthcheckDefinition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ComposeHealthcheckDefinition"/> class.
        /// </summary>
        /// <param name="test">The healthcheck test command.</param>
        /// <param name="interval">The healthcheck interval.</param>
        /// <param name="timeout">The healthcheck timeout.</param>
        /// <param name="retries">The healthcheck retries.</param>
        public ComposeHealthcheckDefinition(IReadOnlyList<string> test, string interval, string timeout, int retries)
        {
            Test = test;
            Interval = interval;
            Timeout = timeout;
            Retries = retries;
        }

        /// <summary>
        /// Gets the healthcheck test command.
        /// </summary>
        public IReadOnlyList<string> Test { get; }

        /// <summary>
        /// Gets the healthcheck interval.
        /// </summary>
        public string Interval { get; }

        /// <summary>
        /// Gets the healthcheck timeout.
        /// </summary>
        public string Timeout { get; }

        /// <summary>
        /// Gets the healthcheck retry count.
        /// </summary>
        public int Retries { get; }
    }
}
