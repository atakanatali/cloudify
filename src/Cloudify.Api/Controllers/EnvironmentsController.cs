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
    private readonly IGetEnvironmentOverviewHandler _overviewHandler;
    private readonly IAddResourceHandler _addResourceHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="EnvironmentsController"/> class.
    /// </summary>
    /// <param name="overviewHandler">The environment overview handler.</param>
    /// <param name="addResourceHandler">The resource creation handler.</param>
    public EnvironmentsController(
        IGetEnvironmentOverviewHandler overviewHandler,
        IAddResourceHandler addResourceHandler)
    {
        _overviewHandler = overviewHandler ?? throw new ArgumentNullException(nameof(overviewHandler));
        _addResourceHandler = addResourceHandler ?? throw new ArgumentNullException(nameof(addResourceHandler));
    }

    /// <summary>
    /// Gets the detailed overview of an environment.
    /// </summary>
    /// <param name="envId">The environment identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The environment overview.</returns>
    [HttpGet("{envId:guid}")]
    [ProducesResponseType(typeof(GetEnvironmentOverviewResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GetEnvironmentOverviewResponse>> GetByIdAsync(Guid envId, CancellationToken cancellationToken)
    {
        Result<GetEnvironmentOverviewResponse> result = await _overviewHandler.HandleAsync(
            new GetEnvironmentOverviewRequest { EnvironmentId = envId },
            cancellationToken);

        if (!result.Success || result.Value is null)
        {
            return ApiProblemDetails.Create(result);
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Adds a resource to an environment.
    /// </summary>
    /// <param name="envId">The environment identifier.</param>
    /// <param name="request">The resource payload.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created resource summary.</returns>
    [HttpPost("{envId:guid}/resources")]
    [ProducesResponseType(typeof(AddResourceResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AddResourceResponse>> AddResourceAsync(
        Guid envId,
        [FromBody] AddResourceRequest? request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return ApiProblemDetails.Create(Result<AddResourceResponse>.Fail(
                ErrorCodes.ValidationFailed,
                "Request payload is required."));
        }

        request.EnvironmentId = envId;

        Result<AddResourceResponse> result = await _addResourceHandler.HandleAsync(request, cancellationToken);

        if (!result.Success || result.Value is null)
        {
            return ApiProblemDetails.Create(result);
        }

        return StatusCode(StatusCodes.Status201Created, result.Value);
    }
}
