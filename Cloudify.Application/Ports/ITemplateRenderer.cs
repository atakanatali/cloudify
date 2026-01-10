namespace Cloudify.Application.Ports;

/// <summary>
/// Defines template rendering operations for environments.
/// </summary>
public interface ITemplateRenderer
{
    /// <summary>
    /// Renders the environment compose configuration.
    /// </summary>
    /// <param name="environmentId">The environment identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The compose yaml text.</returns>
    Task<string> RenderEnvironmentComposeAsync(Guid environmentId, CancellationToken cancellationToken);
}
