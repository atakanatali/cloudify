using Cloudify.Domain.Models;

namespace Cloudify.Application.Dtos;

/// <summary>
/// Represents a summary view of an environment.
/// </summary>
public sealed class EnvironmentSummaryDto
{
    /// <summary>
    /// Gets or sets the environment identifier.
    /// </summary>
    public Guid Id { get; set; }

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

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }
}
