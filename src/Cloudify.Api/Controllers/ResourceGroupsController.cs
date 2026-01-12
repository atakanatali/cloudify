using Cloudify.Application.Dtos;
using Cloudify.Application.Ports;
using Microsoft.AspNetCore.Mvc;

namespace Cloudify.Api.Controllers;

/// <summary>
/// Exposes resource group management endpoints.
/// </summary>
[ApiController]
[Route("api/resource-groups")]
public sealed class ResourceGroupsController : ControllerBase
{
    private readonly ICreateResourceGroupHandler _createResourceGroupHandler;
    private readonly IListResourceGroupsHandler _listResourceGroupsHandler;
    private readonly ICreateEnvironmentHandler _createEnvironmentHandler;
    private readonly IListEnvironmentsHandler _listEnvironmentsHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResourceGroupsController"/> class.
    /// </summary>
    /// <param name="createResourceGroupHandler">The resource group creation handler.</param>
    /// <param name="listResourceGroupsHandler">The resource group list handler.</param>
    /// <param name="createEnvironmentHandler">The environment creation handler.</param>
    /// <param name="listEnvironmentsHandler">The environment list handler.</param>
    public ResourceGroupsController(
        ICreateResourceGroupHandler createResourceGroupHandler,
        IListResourceGroupsHandler listResourceGroupsHandler,
        ICreateEnvironmentHandler createEnvironmentHandler,
        IListEnvironmentsHandler listEnvironmentsHandler)
    {
        _createResourceGroupHandler = createResourceGroupHandler
            ?? throw new ArgumentNullException(nameof(createResourceGroupHandler));
        _listResourceGroupsHandler = listResourceGroupsHandler
            ?? throw new ArgumentNullException(nameof(listResourceGroupsHandler));
        _createEnvironmentHandler = createEnvironmentHandler
            ?? throw new ArgumentNullException(nameof(createEnvironmentHandler));
        _listEnvironmentsHandler = listEnvironmentsHandler
            ?? throw new ArgumentNullException(nameof(listEnvironmentsHandler));
    }

    /// <summary>
    /// Creates a new resource group.
    /// </summary>
    /// <param name="request">The resource group payload.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created resource group.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(CreateResourceGroupResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CreateResourceGroupResponse>> CreateResourceGroupAsync(
        [FromBody] CreateResourceGroupRequest? request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return ApiProblemDetails.Create(Result<CreateResourceGroupResponse>.Fail(
                ErrorCodes.ValidationFailed,
                "Request payload is required."));
        }

        Result<CreateResourceGroupResponse> result = await _createResourceGroupHandler.HandleAsync(request, cancellationToken);

        if (!result.Success || result.Value is null)
        {
            return ApiProblemDetails.Create(result);
        }

        return StatusCode(StatusCodes.Status201Created, result.Value);
    }

    /// <summary>
    /// Lists all resource groups.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The resource group list.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ListResourceGroupsResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ListResourceGroupsResponse>> ListResourceGroupsAsync(CancellationToken cancellationToken)
    {
        Result<ListResourceGroupsResponse> result = await _listResourceGroupsHandler.HandleAsync(
            new ListResourceGroupsRequest(),
            cancellationToken);

        if (!result.Success || result.Value is null)
        {
            return ApiProblemDetails.Create(result);
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Lists environments for a resource group.
    /// </summary>
    /// <param name="rgId">The resource group identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The environment list.</returns>
    [HttpGet("{rgId:guid}/environments")]
    [ProducesResponseType(typeof(ListEnvironmentsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ListEnvironmentsResponse>> ListEnvironmentsAsync(
        Guid rgId,
        CancellationToken cancellationToken)
    {
        Result<ListEnvironmentsResponse> result = await _listEnvironmentsHandler.HandleAsync(
            new ListEnvironmentsRequest { ResourceGroupId = rgId },
            cancellationToken);

        if (!result.Success || result.Value is null)
        {
            return ApiProblemDetails.Create(result);
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Creates a new environment within a resource group.
    /// </summary>
    /// <param name="rgId">The resource group identifier.</param>
    /// <param name="request">The environment payload.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created environment summary.</returns>
    [HttpPost("{rgId:guid}/environments")]
    [ProducesResponseType(typeof(CreateEnvironmentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CreateEnvironmentResponse>> CreateEnvironmentAsync(
        Guid rgId,
        [FromBody] CreateEnvironmentRequest? request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return ApiProblemDetails.Create(Result<CreateEnvironmentResponse>.Fail(
                ErrorCodes.ValidationFailed,
                "Request payload is required."));
        }

        request.ResourceGroupId = rgId;

        Result<CreateEnvironmentResponse> result = await _createEnvironmentHandler.HandleAsync(request, cancellationToken);

        if (!result.Success || result.Value is null)
        {
            return ApiProblemDetails.Create(result);
        }

        return StatusCode(StatusCodes.Status201Created, result.Value);
    }
}
