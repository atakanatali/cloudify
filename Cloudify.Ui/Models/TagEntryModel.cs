namespace Cloudify.Ui.Models;

/// <summary>
/// Represents a single tag entry for resource group creation.
/// </summary>
public sealed class TagEntryModel
{
    /// <summary>
    /// Gets or sets the tag key.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the tag value.
    /// </summary>
    public string Value { get; set; } = string.Empty;
}
