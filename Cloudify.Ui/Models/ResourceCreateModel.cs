using Cloudify.Domain.Models;

namespace Cloudify.Ui.Models;

/// <summary>
/// Captures the input fields required to create a resource.
/// </summary>
public sealed class ResourceCreateModel
{
    /// <summary>
    /// Gets or sets the resource name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the selected resource type.
    /// </summary>
    public ResourceType ResourceType { get; set; } = ResourceType.Postgres;

    /// <summary>
    /// Gets or sets the selected port assignment mode.
    /// </summary>
    public PortMode PortMode { get; set; } = PortMode.Auto;

    /// <summary>
    /// Gets or sets the requested port when manual mode is selected.
    /// </summary>
    public int? RequestedPort { get; set; }

    /// <summary>
    /// Gets or sets the comma-separated list of additional exposed ports.
    /// </summary>
    public string AdditionalPorts { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the application image for app services.
    /// </summary>
    public string? Image { get; set; }

    /// <summary>
    /// Gets or sets the capacity configuration for the resource.
    /// </summary>
    public ResourceCapacityModel Capacity { get; set; } = new();

    /// <summary>
    /// Gets or sets the storage configuration for the resource.
    /// </summary>
    public ResourceStorageModel Storage { get; set; } = new();

    /// <summary>
    /// Gets or sets the credentials for the resource.
    /// </summary>
    public ResourceCredentialModel Credentials { get; set; } = new();

    /// <summary>
    /// Resets the model values back to their defaults.
    /// </summary>
    public void Reset()
    {
        Name = string.Empty;
        ResourceType = ResourceType.Postgres;
        PortMode = PortMode.Auto;
        RequestedPort = null;
        AdditionalPorts = string.Empty;
        Image = null;
        Capacity = new ResourceCapacityModel();
        Storage = new ResourceStorageModel();
        Credentials = new ResourceCredentialModel();
    }
}

/// <summary>
/// Captures capacity settings for a resource.
/// </summary>
public sealed class ResourceCapacityModel
{
    /// <summary>
    /// Gets or sets the CPU limit requested for the resource.
    /// </summary>
    public int? CpuLimit { get; set; }

    /// <summary>
    /// Gets or sets the memory limit requested for the resource.
    /// </summary>
    public int? MemoryLimit { get; set; }

    /// <summary>
    /// Gets or sets the replica count for the resource.
    /// </summary>
    public int Replicas { get; set; } = 1;

    /// <summary>
    /// Gets or sets any additional notes for the capacity request.
    /// </summary>
    public string? Notes { get; set; }
}

/// <summary>
/// Captures storage settings for a resource.
/// </summary>
public sealed class ResourceStorageModel
{
    /// <summary>
    /// Gets or sets the volume name.
    /// </summary>
    public string VolumeName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the storage size in gigabytes.
    /// </summary>
    public int SizeGb { get; set; } = 10;

    /// <summary>
    /// Gets or sets the mount path.
    /// </summary>
    public string MountPath { get; set; } = "/data";

    /// <summary>
    /// Gets or sets a value indicating whether the storage should be persistent.
    /// </summary>
    public bool IsPersistent { get; set; } = true;
}

/// <summary>
/// Captures credential settings for a resource.
/// </summary>
public sealed class ResourceCredentialModel
{
    /// <summary>
    /// Gets or sets the credential username.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the credential password.
    /// </summary>
    public string Password { get; set; } = string.Empty;
}
