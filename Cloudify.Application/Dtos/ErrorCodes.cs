namespace Cloudify.Application.Dtos;

/// <summary>
/// Defines shared error code values.
/// </summary>
public static class ErrorCodes
{
    /// <summary>
    /// Indicates validation failures.
    /// </summary>
    public const string ValidationFailed = "validation_failed";

    /// <summary>
    /// Indicates a requested entity was not found.
    /// </summary>
    public const string NotFound = "not_found";

    /// <summary>
    /// Indicates a conflict or duplicate.
    /// </summary>
    public const string Conflict = "conflict";

    /// <summary>
    /// Indicates an invalid operation.
    /// </summary>
    public const string InvalidOperation = "invalid_operation";
}
