namespace Cloudify.Application.Dtos;

/// <summary>
/// Represents a request to retrieve environment overview data.
/// </summary>
public sealed class GetEnvironmentOverviewRequest
{
    /// <summary>
    /// Gets or sets the environment identifier.
    /// </summary>
    public Guid EnvironmentId { get; set; }
}

/// <summary>
/// Represents the response for retrieving environment overview data.
/// </summary>
public sealed class GetEnvironmentOverviewResponse
{
    /// <summary>
    /// Gets or sets the environment overview.
    /// </summary>
    public EnvironmentOverviewDto Overview { get; set; } = new();
}
