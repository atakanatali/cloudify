using System.Net.Http.Json;
using Cloudify.Application.Dtos;
using Cloudify.Ui.Models;

namespace Cloudify.Ui.Services;

/// <summary>
/// Provides typed access to the Cloudify API.
/// </summary>
public sealed class CloudifyApiClient
{
    /// <summary>
    /// Defines the named HTTP client used for API calls.
    /// </summary>
    public const string ClientName = "CloudifyApi";

    private readonly IHttpClientFactory _httpClientFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="CloudifyApiClient"/> class.
    /// </summary>
    /// <param name="httpClientFactory">The HTTP client factory.</param>
    public CloudifyApiClient(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    }

    /// <summary>
    /// Retrieves all resource groups.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The resource group list result.</returns>
    public async Task<ApiResult<IReadOnlyList<ResourceGroupSummaryDto>>> ListResourceGroupsAsync(CancellationToken cancellationToken)
    {
        HttpClient client = CreateClient();
        using HttpResponseMessage response = await client.GetAsync("api/resource-groups", cancellationToken);
        return await ReadResponseAsync<ListResourceGroupsResponse, IReadOnlyList<ResourceGroupSummaryDto>>(
            response,
            payload => payload.ResourceGroups,
            cancellationToken);
    }

    /// <summary>
    /// Creates a new resource group.
    /// </summary>
    /// <param name="request">The create request payload.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The create result.</returns>
    public async Task<ApiResult<ResourceGroupSummaryDto>> CreateResourceGroupAsync(
        CreateResourceGroupRequest request,
        CancellationToken cancellationToken)
    {
        HttpClient client = CreateClient();
        using HttpResponseMessage response = await client.PostAsJsonAsync("api/resource-groups", request, cancellationToken);
        return await ReadResponseAsync<CreateResourceGroupResponse, ResourceGroupSummaryDto>(
            response,
            payload => payload.ResourceGroup,
            cancellationToken);
    }

    /// <summary>
    /// Lists environments for a resource group.
    /// </summary>
    /// <param name="resourceGroupId">The resource group identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The environment list result.</returns>
    public async Task<ApiResult<IReadOnlyList<EnvironmentSummaryDto>>> ListEnvironmentsAsync(
        Guid resourceGroupId,
        CancellationToken cancellationToken)
    {
        HttpClient client = CreateClient();
        string path = $"api/resource-groups/{resourceGroupId}/environments";
        using HttpResponseMessage response = await client.GetAsync(path, cancellationToken);
        return await ReadResponseAsync<ListEnvironmentsResponse, IReadOnlyList<EnvironmentSummaryDto>>(
            response,
            payload => payload.Environments,
            cancellationToken);
    }

    /// <summary>
    /// Creates a new environment within a resource group.
    /// </summary>
    /// <param name="resourceGroupId">The resource group identifier.</param>
    /// <param name="request">The create request payload.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The create result.</returns>
    public async Task<ApiResult<EnvironmentSummaryDto>> CreateEnvironmentAsync(
        Guid resourceGroupId,
        CreateEnvironmentRequest request,
        CancellationToken cancellationToken)
    {
        HttpClient client = CreateClient();
        string path = $"api/resource-groups/{resourceGroupId}/environments";
        using HttpResponseMessage response = await client.PostAsJsonAsync(path, request, cancellationToken);
        return await ReadResponseAsync<CreateEnvironmentResponse, EnvironmentSummaryDto>(
            response,
            payload => payload.Environment,
            cancellationToken);
    }

    /// <summary>
    /// Retrieves detailed overview information for an environment.
    /// </summary>
    /// <param name="environmentId">The environment identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The environment overview result.</returns>
    public async Task<ApiResult<EnvironmentOverviewDto>> GetEnvironmentOverviewAsync(
        Guid environmentId,
        CancellationToken cancellationToken)
    {
        HttpClient client = CreateClient();
        string path = $"api/environments/{environmentId}";
        using HttpResponseMessage response = await client.GetAsync(path, cancellationToken);
        return await ReadResponseAsync<GetEnvironmentOverviewResponse, EnvironmentOverviewDto>(
            response,
            payload => payload.Overview,
            cancellationToken);
    }

    /// <summary>
    /// Adds a new resource to an environment.
    /// </summary>
    /// <param name="environmentId">The environment identifier.</param>
    /// <param name="request">The resource payload.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The add resource result.</returns>
    public async Task<ApiResult<ResourceSummaryDto>> AddResourceAsync(
        Guid environmentId,
        AddResourceRequest request,
        CancellationToken cancellationToken)
    {
        HttpClient client = CreateClient();
        string path = $"api/environments/{environmentId}/resources";
        using HttpResponseMessage response = await client.PostAsJsonAsync(path, request, cancellationToken);
        return await ReadResponseAsync<AddResourceResponse, ResourceSummaryDto>(
            response,
            payload => payload.Resource,
            cancellationToken);
    }

    /// <summary>
    /// Starts a resource.
    /// </summary>
    /// <param name="resourceId">The resource identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The start result.</returns>
    public async Task<ApiResult<StartResourceResponse>> StartResourceAsync(Guid resourceId, CancellationToken cancellationToken)
    {
        HttpClient client = CreateClient();
        string path = $"api/resources/{resourceId}/start";
        using HttpResponseMessage response = await client.PostAsync(path, content: null, cancellationToken);
        return await ReadResponseAsync<StartResourceResponse>(response, cancellationToken);
    }

    /// <summary>
    /// Stops a resource.
    /// </summary>
    /// <param name="resourceId">The resource identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The stop result.</returns>
    public async Task<ApiResult<StopResourceResponse>> StopResourceAsync(Guid resourceId, CancellationToken cancellationToken)
    {
        HttpClient client = CreateClient();
        string path = $"api/resources/{resourceId}/stop";
        using HttpResponseMessage response = await client.PostAsync(path, content: null, cancellationToken);
        return await ReadResponseAsync<StopResourceResponse>(response, cancellationToken);
    }

    /// <summary>
    /// Restarts a resource.
    /// </summary>
    /// <param name="resourceId">The resource identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The restart result.</returns>
    public async Task<ApiResult<RestartResourceResponse>> RestartResourceAsync(Guid resourceId, CancellationToken cancellationToken)
    {
        HttpClient client = CreateClient();
        string path = $"api/resources/{resourceId}/restart";
        using HttpResponseMessage response = await client.PostAsync(path, content: null, cancellationToken);
        return await ReadResponseAsync<RestartResourceResponse>(response, cancellationToken);
    }

    /// <summary>
    /// Scales a resource to the specified replica count.
    /// </summary>
    /// <param name="resourceId">The resource identifier.</param>
    /// <param name="replicas">The desired replica count.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The scale result.</returns>
    public async Task<ApiResult<ScaleResourceResponse>> ScaleResourceAsync(
        Guid resourceId,
        int replicas,
        CancellationToken cancellationToken)
    {
        HttpClient client = CreateClient();
        string path = $"api/resources/{resourceId}/scale";
        var payload = new ScaleResourceRequest
        {
            ResourceId = resourceId,
            Replicas = replicas,
        };
        using HttpResponseMessage response = await client.PostAsJsonAsync(path, payload, cancellationToken);
        return await ReadResponseAsync<ScaleResourceResponse>(response, cancellationToken);
    }

    /// <summary>
    /// Deletes a resource.
    /// </summary>
    /// <param name="resourceId">The resource identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The delete result.</returns>
    public async Task<ApiResult<DeleteResourceResponse>> DeleteResourceAsync(Guid resourceId, CancellationToken cancellationToken)
    {
        HttpClient client = CreateClient();
        string path = $"api/resources/{resourceId}";
        using HttpResponseMessage response = await client.DeleteAsync(path, cancellationToken);
        return await ReadResponseAsync<DeleteResourceResponse>(response, cancellationToken);
    }

    /// <summary>
    /// Retrieves the latest resource logs.
    /// </summary>
    /// <param name="resourceId">The resource identifier.</param>
    /// <param name="tail">The number of lines to tail.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The logs result.</returns>
    public async Task<ApiResult<GetResourceLogsResponse>> GetResourceLogsAsync(
        Guid resourceId,
        int tail,
        CancellationToken cancellationToken)
    {
        HttpClient client = CreateClient();
        string path = $"api/resources/{resourceId}/logs?tail={tail}";
        using HttpResponseMessage response = await client.GetAsync(path, cancellationToken);
        return await ReadResponseAsync<GetResourceLogsResponse>(response, cancellationToken);
    }

    /// <summary>
    /// Retrieves the current health status for a resource.
    /// </summary>
    /// <param name="resourceId">The resource identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The health status result.</returns>
    public async Task<ApiResult<GetResourceHealthResponse>> GetResourceHealthAsync(Guid resourceId, CancellationToken cancellationToken)
    {
        HttpClient client = CreateClient();
        string path = $"api/resources/{resourceId}/health";
        using HttpResponseMessage response = await client.GetAsync(path, cancellationToken);
        return await ReadResponseAsync<GetResourceHealthResponse>(response, cancellationToken);
    }

    /// <summary>
    /// Creates the named API client instance.
    /// </summary>
    /// <returns>The configured HTTP client.</returns>
    private HttpClient CreateClient()
    {
        return _httpClientFactory.CreateClient(ClientName);
    }

    /// <summary>
    /// Reads an API response with a typed payload and selector.
    /// </summary>
    /// <typeparam name="TResponse">The response payload type.</typeparam>
    /// <typeparam name="TValue">The selected value type.</typeparam>
    /// <param name="response">The HTTP response.</param>
    /// <param name="selector">The selector used to map the payload.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The API result.</returns>
    private static async Task<ApiResult<TValue>> ReadResponseAsync<TResponse, TValue>(
        HttpResponseMessage response,
        Func<TResponse, TValue> selector,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            TResponse? payload = await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken: cancellationToken);
            if (payload is null)
            {
                return ApiResult<TValue>.Fail("The API returned an empty response.");
            }

            return ApiResult<TValue>.Ok(selector(payload));
        }

        string error = await ReadErrorAsync(response, cancellationToken);
        return ApiResult<TValue>.Fail(error);
    }

    /// <summary>
    /// Reads an API response with a typed payload.
    /// </summary>
    /// <typeparam name="TResponse">The response payload type.</typeparam>
    /// <param name="response">The HTTP response.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The API result.</returns>
    private static async Task<ApiResult<TResponse>> ReadResponseAsync<TResponse>(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            TResponse? payload = await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken: cancellationToken);
            if (payload is null)
            {
                return ApiResult<TResponse>.Fail("The API returned an empty response.");
            }

            return ApiResult<TResponse>.Ok(payload);
        }

        string error = await ReadErrorAsync(response, cancellationToken);
        return ApiResult<TResponse>.Fail(error);
    }

    /// <summary>
    /// Attempts to read an error message from the response body.
    /// </summary>
    /// <param name="response">The HTTP response.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The extracted error message.</returns>
    private static async Task<string> ReadErrorAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        string raw = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!string.IsNullOrWhiteSpace(raw))
        {
            return raw;
        }

        return response.ReasonPhrase ?? "The API request failed.";
    }
}
