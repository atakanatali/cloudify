using Cloudify.Application.Dtos;

namespace Cloudify.Application.Ports;

/// <summary>
/// Defines the use case for creating an environment.
/// </summary>
public interface ICreateEnvironmentHandler
{
    /// <summary>
    /// Handles the create environment request.
    /// </summary>
    /// <param name="request">The request payload.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result containing the response.</returns>
    Task<Result<CreateEnvironmentResponse>> HandleAsync(CreateEnvironmentRequest request, CancellationToken cancellationToken);
}
