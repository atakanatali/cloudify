namespace Cloudify.Application.Dtos;

/// <summary>
/// Represents a request to list environments for a resource group.
/// </summary>
public sealed class ListEnvironmentsRequest
{
    /// <summary>
    /// Gets or sets the resource group identifier.
    /// </summary>
    public Guid ResourceGroupId { get; set; }
}

/// <summary>
/// Represents the response for listing environments.
/// </summary>
public sealed class ListEnvironmentsResponse
{
    /// <summary>
    /// Gets or sets the environment summaries.
    /// </summary>
    public IReadOnlyList<EnvironmentSummaryDto> Environments { get; set; } = Array.Empty<EnvironmentSummaryDto>();
}
