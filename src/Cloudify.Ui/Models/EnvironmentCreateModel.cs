using CloudifyDomainEnvironmentName = Cloudify.Domain.Models.EnvironmentName;
using Cloudify.Domain.Models;

namespace Cloudify.Ui.Models;

/// <summary>
/// Captures the input fields required to create an environment.
/// </summary>
public sealed class EnvironmentCreateModel
{
    /// <summary>
    /// Gets or sets the environment name.
    /// </summary>
    public CloudifyDomainEnvironmentName Name { get; set; } = CloudifyDomainEnvironmentName.Dev;

    /// <summary>
    /// Gets or sets the network mode for the environment.
    /// </summary>
    public NetworkMode NetworkMode { get; set; } = NetworkMode.Bridge;

    /// <summary>
    /// Gets or sets the optional base domain name.
    /// </summary>
    public string? BaseDomain { get; set; }

    /// <summary>
    /// Resets the model values back to their defaults.
    /// </summary>
    public void Reset()
    {
        Name = CloudifyDomainEnvironmentName.Dev;
        NetworkMode = NetworkMode.Bridge;
        BaseDomain = null;
    }
}
