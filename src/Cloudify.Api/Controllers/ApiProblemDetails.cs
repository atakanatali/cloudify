using Cloudify.Application.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Cloudify.Api.Controllers;

/// <summary>
/// Creates RFC 7807 problem details for API error responses.
/// </summary>
internal static class ApiProblemDetails
{
    /// <summary>
    /// Creates a problem details response for a failed result.
    /// </summary>
    /// <param name="result">The failed result.</param>
    /// <returns>The action result containing problem details.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the result is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the result indicates success.</exception>
    public static ObjectResult Create(Result result)
    {
        if (result is null)
        {
            throw new ArgumentNullException(nameof(result));
        }

        if (result.Success)
        {
            throw new InvalidOperationException("Cannot create problem details from a successful result.");
        }

        string? code = result.Error?.Code;
        int statusCode = GetStatusCode(code);
        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = GetTitle(code),
            Detail = result.Error?.Message,
            Type = $"https://cloudify.api/errors/{code ?? "unknown"}",
        };

        if (!string.IsNullOrWhiteSpace(code))
        {
            problem.Extensions["code"] = code;
        }

        return new ObjectResult(problem)
        {
            StatusCode = statusCode,
            DeclaredType = typeof(ProblemDetails),
        };
    }

    /// <summary>
    /// Maps an error code to a corresponding HTTP status code.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <returns>The HTTP status code.</returns>
    private static int GetStatusCode(string? code)
    {
        return code switch
        {
            ErrorCodes.ValidationFailed => StatusCodes.Status400BadRequest,
            ErrorCodes.NotFound => StatusCodes.Status404NotFound,
            ErrorCodes.Conflict => StatusCodes.Status409Conflict,
            ErrorCodes.InvalidOperation => StatusCodes.Status422UnprocessableEntity,
            _ => StatusCodes.Status500InternalServerError,
        };
    }

    /// <summary>
    /// Maps an error code to a short, human-readable title.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <returns>The title string.</returns>
    private static string GetTitle(string? code)
    {
        return code switch
        {
            ErrorCodes.ValidationFailed => "Validation failed",
            ErrorCodes.NotFound => "Resource not found",
            ErrorCodes.Conflict => "Conflict",
            ErrorCodes.InvalidOperation => "Invalid operation",
            _ => "Unexpected error",
        };
    }
}
