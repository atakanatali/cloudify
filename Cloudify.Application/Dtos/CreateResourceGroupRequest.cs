namespace Cloudify.Application.Dtos;

/// <summary>
/// Represents a request to create a resource group.
/// </summary>
public sealed class CreateResourceGroupRequest
{
    /// <summary>
    /// Gets or sets the resource group name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the tags applied to the resource group.
    /// </summary>
    public IDictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();
}

/// <summary>
/// Represents the response for creating a resource group.
/// </summary>
public sealed class CreateResourceGroupResponse
{
    /// <summary>
    /// Gets or sets the created resource group summary.
    /// </summary>
    public ResourceGroupSummaryDto ResourceGroup { get; set; } = new();
}
