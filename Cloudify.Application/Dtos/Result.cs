namespace Cloudify.Application.Dtos;

/// <summary>
/// Represents the outcome of an operation without a payload.
/// </summary>
public class Result
{
    /// <summary>
    /// Gets or sets a value indicating whether the operation succeeded.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the error payload when the operation fails.
    /// </summary>
    public ErrorDto? Error { get; set; }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    /// <returns>The successful result.</returns>
    public static Result Ok()
    {
        return new Result { Success = true };
    }

    /// <summary>
    /// Creates a failed result.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    /// <returns>The failed result.</returns>
    public static Result Fail(string code, string message)
    {
        return new Result
        {
            Success = false,
            Error = new ErrorDto
            {
                Code = code,
                Message = message,
            },
        };
    }
}

/// <summary>
/// Represents the outcome of an operation with a payload.
/// </summary>
/// <typeparam name="T">The payload type.</typeparam>
public sealed class Result<T> : Result
{
    /// <summary>
    /// Gets or sets the payload value.
    /// </summary>
    public T? Value { get; set; }

    /// <summary>
    /// Creates a successful result with a value.
    /// </summary>
    /// <param name="value">The payload value.</param>
    /// <returns>The successful result.</returns>
    public static Result<T> Ok(T value)
    {
        return new Result<T>
        {
            Success = true,
            Value = value,
        };
    }

    /// <summary>
    /// Creates a failed result.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    /// <returns>The failed result.</returns>
    public static new Result<T> Fail(string code, string message)
    {
        return new Result<T>
        {
            Success = false,
            Error = new ErrorDto
            {
                Code = code,
                Message = message,
            },
        };
    }
}
