using Cloudify.Domain.Models;

namespace Cloudify.Application.Dtos;

/// <summary>
/// Represents a request to create an environment.
/// </summary>
public sealed class CreateEnvironmentRequest
{
    /// <summary>
    /// Gets or sets the resource group identifier.
    /// </summary>
    public Guid ResourceGroupId { get; set; }

    /// <summary>
    /// Gets or sets the environment name.
    /// </summary>
    public EnvironmentName Name { get; set; }

    /// <summary>
    /// Gets or sets the network mode.
    /// </summary>
    public NetworkMode NetworkMode { get; set; }

    /// <summary>
    /// Gets or sets the optional base domain.
    /// </summary>
    public string? BaseDomain { get; set; }
}

/// <summary>
/// Represents the response for creating an environment.
/// </summary>
public sealed class CreateEnvironmentResponse
{
    /// <summary>
    /// Gets or sets the created environment summary.
    /// </summary>
    public EnvironmentSummaryDto Environment { get; set; } = new();
}
