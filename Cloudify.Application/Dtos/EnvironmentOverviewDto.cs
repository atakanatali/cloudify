namespace Cloudify.Application.Dtos;

/// <summary>
/// Represents a detailed overview of an environment.
/// </summary>
public sealed class EnvironmentOverviewDto
{
    /// <summary>
    /// Gets or sets the environment summary.
    /// </summary>
    public EnvironmentSummaryDto Environment { get; set; } = new();

    /// <summary>
    /// Gets or sets the resources within the environment.
    /// </summary>
    public IReadOnlyList<ResourceSummaryDto> Resources { get; set; } = Array.Empty<ResourceSummaryDto>();

    /// <summary>
    /// Gets or sets the rendered compose document.
    /// </summary>
    public string ComposeYaml { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the host profile information.
    /// </summary>
    public HostProfileDto HostProfile { get; set; } = new();
}
