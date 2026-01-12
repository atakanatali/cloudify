namespace Cloudify.Ui.Models;

/// <summary>
/// Represents a typed result for API calls.
/// </summary>
/// <typeparam name="T">The payload type returned by the API.</typeparam>
public sealed class ApiResult<T>
{
    /// <summary>
    /// Gets a value indicating whether the request succeeded.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Gets the response payload when the request succeeds.
    /// </summary>
    public T? Value { get; init; }

    /// <summary>
    /// Gets the error message when the request fails.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    /// <param name="value">The response payload.</param>
    /// <returns>The successful result.</returns>
    public static ApiResult<T> Ok(T value)
    {
        return new ApiResult<T>
        {
            Success = true,
            Value = value,
        };
    }

    /// <summary>
    /// Creates a failure result.
    /// </summary>
    /// <param name="errorMessage">The error message.</param>
    /// <returns>The failure result.</returns>
    public static ApiResult<T> Fail(string errorMessage)
    {
        return new ApiResult<T>
        {
            Success = false,
            ErrorMessage = errorMessage,
        };
    }
}
