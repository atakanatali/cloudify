namespace Cloudify.Application.Dtos;

/// <summary>
/// Represents a summary view of a resource group.
/// </summary>
public sealed class ResourceGroupSummaryDto
{
    /// <summary>
    /// Gets or sets the resource group identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the resource group name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the tags associated with the resource group.
    /// </summary>
    public IReadOnlyDictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();
}
