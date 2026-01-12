namespace Cloudify.Domain.Models;

/// <summary>
/// Describes networking modes for environment resources.
/// </summary>
public enum NetworkMode
{
    /// <summary>
    /// Indicates the bridge network mode.
    /// </summary>
    Bridge = 1,

    /// <summary>
    /// Indicates the host network mode.
    /// </summary>
    Host = 2,

    /// <summary>
    /// Indicates no dedicated network mode.
    /// </summary>
    None = 3
}
