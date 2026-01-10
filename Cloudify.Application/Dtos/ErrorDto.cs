namespace Cloudify.Application.Dtos;

/// <summary>
/// Represents a standardized error payload.
/// </summary>
public sealed class ErrorDto
{
    /// <summary>
    /// Gets or sets the error code identifier.
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the human-readable error message.
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
