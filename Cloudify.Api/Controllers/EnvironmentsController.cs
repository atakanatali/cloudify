using Cloudify.Application.Dtos;
using Cloudify.Application.Ports;
using Microsoft.AspNetCore.Mvc;

namespace Cloudify.Api.Controllers;

/// <summary>
/// Exposes environment management endpoints.
/// </summary>
[ApiController]
[Route("api/environments")]
public sealed class EnvironmentsController : ControllerBase
{
    private readonly IEnvironmentService _service;

    /// <summary>
    /// Initializes a new instance of the <see cref="EnvironmentsController"/> class.
    /// </summary>
    /// <param name="service">The environment service.</param>
    public EnvironmentsController(IEnvironmentService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    /// <summary>
    /// Gets all environments.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The list of environments.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CloudEnvironmentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CloudEnvironmentDto>>> GetAllAsync(CancellationToken cancellationToken)
    {
        IReadOnlyList<CloudEnvironmentDto> environments = await _service.GetAllAsync(cancellationToken);
        return Ok(environments);
    }

    /// <summary>
    /// Creates a new environment.
    /// </summary>
    /// <param name="request">The environment payload.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> CreateAsync([FromBody] CloudEnvironmentDto request, CancellationToken cancellationToken)
    {
        await _service.CreateAsync(request, cancellationToken);
        return Accepted();
    }
}
