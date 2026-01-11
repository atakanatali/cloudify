namespace Cloudify.Ui.Models;

/// <summary>
/// Captures the input fields required to create a resource group.
/// </summary>
public sealed class ResourceGroupCreateModel
{
    /// <summary>
    /// Gets or sets the resource group name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets the tag entries provided for the resource group.
    /// </summary>
    public List<TagEntryModel> Tags { get; } = new();

    /// <summary>
    /// Clears the input fields to their defaults.
    /// </summary>
    public void Reset()
    {
        Name = string.Empty;
        Tags.Clear();
    }
}
