using Cloudify.Application.Dtos;
using Cloudify.Application.Ports;
using Microsoft.AspNetCore.Mvc;

namespace Cloudify.Api.Controllers;

/// <summary>
/// Exposes resource lifecycle and inspection endpoints.
/// </summary>
[ApiController]
[Route("api/resources")]
public sealed class ResourcesController : ControllerBase
{
    private readonly IStartResourceHandler _startResourceHandler;
    private readonly IStopResourceHandler _stopResourceHandler;
    private readonly IRestartResourceHandler _restartResourceHandler;
    private readonly IScaleResourceHandler _scaleResourceHandler;
    private readonly IGetResourceLogsHandler _getResourceLogsHandler;
    private readonly IGetResourceHealthHandler _getResourceHealthHandler;
    private readonly IDeleteResourceHandler _deleteResourceHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResourcesController"/> class.
    /// </summary>
    /// <param name="startResourceHandler">The resource start handler.</param>
    /// <param name="stopResourceHandler">The resource stop handler.</param>
    /// <param name="restartResourceHandler">The resource restart handler.</param>
    /// <param name="scaleResourceHandler">The resource scale handler.</param>
    /// <param name="getResourceLogsHandler">The resource log handler.</param>
    /// <param name="getResourceHealthHandler">The resource health handler.</param>
    /// <param name="deleteResourceHandler">The resource deletion handler.</param>
    public ResourcesController(
        IStartResourceHandler startResourceHandler,
        IStopResourceHandler stopResourceHandler,
        IRestartResourceHandler restartResourceHandler,
        IScaleResourceHandler scaleResourceHandler,
        IGetResourceLogsHandler getResourceLogsHandler,
        IGetResourceHealthHandler getResourceHealthHandler,
        IDeleteResourceHandler deleteResourceHandler)
    {
        _startResourceHandler = startResourceHandler ?? throw new ArgumentNullException(nameof(startResourceHandler));
        _stopResourceHandler = stopResourceHandler ?? throw new ArgumentNullException(nameof(stopResourceHandler));
        _restartResourceHandler = restartResourceHandler ?? throw new ArgumentNullException(nameof(restartResourceHandler));
        _scaleResourceHandler = scaleResourceHandler ?? throw new ArgumentNullException(nameof(scaleResourceHandler));
        _getResourceLogsHandler = getResourceLogsHandler ?? throw new ArgumentNullException(nameof(getResourceLogsHandler));
        _getResourceHealthHandler = getResourceHealthHandler ?? throw new ArgumentNullException(nameof(getResourceHealthHandler));
        _deleteResourceHandler = deleteResourceHandler ?? throw new ArgumentNullException(nameof(deleteResourceHandler));
    }

    /// <summary>
    /// Starts a resource.
    /// </summary>
    /// <param name="resId">The resource identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated resource state.</returns>
    [HttpPost("{resId:guid}/start")]
    [ProducesResponseType(typeof(StartResourceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StartResourceResponse>> StartAsync(Guid resId, CancellationToken cancellationToken)
    {
        Result<StartResourceResponse> result = await _startResourceHandler.HandleAsync(
            new StartResourceRequest { ResourceId = resId },
            cancellationToken);

        if (!result.Success || result.Value is null)
        {
            return ApiProblemDetails.Create(result);
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Stops a resource.
    /// </summary>
    /// <param name="resId">The resource identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated resource state.</returns>
    [HttpPost("{resId:guid}/stop")]
    [ProducesResponseType(typeof(StopResourceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StopResourceResponse>> StopAsync(Guid resId, CancellationToken cancellationToken)
    {
        Result<StopResourceResponse> result = await _stopResourceHandler.HandleAsync(
            new StopResourceRequest { ResourceId = resId },
            cancellationToken);

        if (!result.Success || result.Value is null)
        {
            return ApiProblemDetails.Create(result);
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Restarts a resource.
    /// </summary>
    /// <param name="resId">The resource identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated resource state.</returns>
    [HttpPost("{resId:guid}/restart")]
    [ProducesResponseType(typeof(RestartResourceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RestartResourceResponse>> RestartAsync(Guid resId, CancellationToken cancellationToken)
    {
        Result<RestartResourceResponse> result = await _restartResourceHandler.HandleAsync(
            new RestartResourceRequest { ResourceId = resId },
            cancellationToken);

        if (!result.Success || result.Value is null)
        {
            return ApiProblemDetails.Create(result);
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Scales a resource to the specified replica count.
    /// </summary>
    /// <param name="resId">The resource identifier.</param>
    /// <param name="request">The scale payload.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated scale response.</returns>
    [HttpPost("{resId:guid}/scale")]
    [ProducesResponseType(typeof(ScaleResourceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ScaleResourceResponse>> ScaleAsync(
        Guid resId,
        [FromBody] ScaleResourceRequest? request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return ApiProblemDetails.Create(Result<ScaleResourceResponse>.Fail(
                ErrorCodes.ValidationFailed,
                "Request payload is required."));
        }

        request.ResourceId = resId;

        Result<ScaleResourceResponse> result = await _scaleResourceHandler.HandleAsync(request, cancellationToken);

        if (!result.Success || result.Value is null)
        {
            return ApiProblemDetails.Create(result);
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Retrieves resource logs.
    /// </summary>
    /// <param name="resId">The resource identifier.</param>
    /// <param name="tail">The number of log lines to return.</param>
    /// <param name="service">The optional service name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The log output.</returns>
    [HttpGet("{resId:guid}/logs")]
    [ProducesResponseType(typeof(GetResourceLogsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GetResourceLogsResponse>> GetLogsAsync(
        Guid resId,
        [FromQuery] int tail = 200,
        [FromQuery] string? service = null,
        CancellationToken cancellationToken = default)
    {
        var request = new GetResourceLogsRequest
        {
            ResourceId = resId,
            Tail = tail,
            ServiceName = service,
        };

        Result<GetResourceLogsResponse> result = await _getResourceLogsHandler.HandleAsync(request, cancellationToken);

        if (!result.Success || result.Value is null)
        {
            return ApiProblemDetails.Create(result);
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Gets the health of a resource.
    /// </summary>
    /// <param name="resId">The resource identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The resource health response.</returns>
    [HttpGet("{resId:guid}/health")]
    [ProducesResponseType(typeof(GetResourceHealthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GetResourceHealthResponse>> GetHealthAsync(
        Guid resId,
        CancellationToken cancellationToken)
    {
        Result<GetResourceHealthResponse> result = await _getResourceHealthHandler.HandleAsync(
            new GetResourceHealthRequest { ResourceId = resId },
            cancellationToken);

        if (!result.Success || result.Value is null)
        {
            return ApiProblemDetails.Create(result);
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Deletes a resource.
    /// </summary>
    /// <param name="resId">The resource identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The deletion confirmation.</returns>
    [HttpDelete("{resId:guid}")]
    [ProducesResponseType(typeof(DeleteResourceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DeleteResourceResponse>> DeleteAsync(Guid resId, CancellationToken cancellationToken)
    {
        Result<DeleteResourceResponse> result = await _deleteResourceHandler.HandleAsync(
            new DeleteResourceRequest { ResourceId = resId },
            cancellationToken);

        if (!result.Success || result.Value is null)
        {
            return ApiProblemDetails.Create(result);
        }

        return Ok(result.Value);
    }
}
