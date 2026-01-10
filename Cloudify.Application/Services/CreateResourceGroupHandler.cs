using Cloudify.Application.Dtos;
using Cloudify.Application.Ports;
using Cloudify.Domain.Models;

namespace Cloudify.Application.Services;

/// <summary>
/// Handles resource group creation requests.
/// </summary>
public sealed class CreateResourceGroupHandler : ICreateResourceGroupHandler
{
    private readonly IStateStore _stateStore;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateResourceGroupHandler"/> class.
    /// </summary>
    /// <param name="stateStore">The state store.</param>
    public CreateResourceGroupHandler(IStateStore stateStore)
    {
        _stateStore = stateStore ?? throw new ArgumentNullException(nameof(stateStore));
    }

    /// <inheritdoc />
    public async Task<Result<CreateResourceGroupResponse>> HandleAsync(CreateResourceGroupRequest request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return Result<CreateResourceGroupResponse>.Fail(ErrorCodes.ValidationFailed, "Request payload is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Result<CreateResourceGroupResponse>.Fail(ErrorCodes.ValidationFailed, "Resource group name is required.");
        }

        IDictionary<string, string> tags = request.Tags ?? new Dictionary<string, string>();
        var resourceGroup = new ResourceGroup(Guid.NewGuid(), request.Name, DateTimeOffset.UtcNow, tags);
        await _stateStore.AddResourceGroupAsync(resourceGroup, cancellationToken);

        return Result<CreateResourceGroupResponse>.Ok(new CreateResourceGroupResponse
        {
            ResourceGroup = new ResourceGroupSummaryDto
            {
                Id = resourceGroup.Id,
                Name = resourceGroup.Name,
                CreatedAt = resourceGroup.CreatedAt,
                Tags = new Dictionary<string, string>(resourceGroup.Tags),
            },
        });
    }
}
