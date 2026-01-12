namespace Cloudify.Application.Dtos;

/// <summary>
/// Represents port exposure configuration.
/// </summary>
public sealed class PortPolicyDto
{
    /// <summary>
    /// Gets or sets the exposed ports list.
    /// </summary>
    public IReadOnlyList<int> ExposedPorts { get; set; } = Array.Empty<int>();
}
