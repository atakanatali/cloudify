using Cloudify.Application.Dtos;
using Cloudify.Application.Ports;

namespace Cloudify.Application.Services;

/// <summary>
/// Handles listing resource groups.
/// </summary>
public sealed class ListResourceGroupsHandler : IListResourceGroupsHandler
{
    private readonly IStateStore _stateStore;

    /// <summary>
    /// Initializes a new instance of the <see cref="ListResourceGroupsHandler"/> class.
    /// </summary>
    /// <param name="stateStore">The state store.</param>
    public ListResourceGroupsHandler(IStateStore stateStore)
    {
        _stateStore = stateStore ?? throw new ArgumentNullException(nameof(stateStore));
    }

    /// <inheritdoc />
    public async Task<Result<ListResourceGroupsResponse>> HandleAsync(ListResourceGroupsRequest request, CancellationToken cancellationToken)
    {
        IReadOnlyList<Cloudify.Domain.Models.ResourceGroup> groups = await _stateStore.ListResourceGroupsAsync(cancellationToken);
        var response = new ListResourceGroupsResponse
        {
            ResourceGroups = groups
                .Select(group => new ResourceGroupSummaryDto
                {
                    Id = group.Id,
                    Name = group.Name,
                    CreatedAt = group.CreatedAt,
                    Tags = new Dictionary<string, string>(group.Tags),
                })
                .ToArray(),
        };

        return Result<ListResourceGroupsResponse>.Ok(response);
    }
}
