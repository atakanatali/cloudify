namespace Cloudify.Application.Dtos;

/// <summary>
/// Represents a request to list resource groups.
/// </summary>
public sealed class ListResourceGroupsRequest
{
}

/// <summary>
/// Represents the response for listing resource groups.
/// </summary>
public sealed class ListResourceGroupsResponse
{
    /// <summary>
    /// Gets or sets the resource groups.
    /// </summary>
    public IReadOnlyList<ResourceGroupSummaryDto> ResourceGroups { get; set; } = Array.Empty<ResourceGroupSummaryDto>();
}
