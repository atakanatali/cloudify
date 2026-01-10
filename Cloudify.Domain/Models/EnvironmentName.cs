namespace Cloudify.Domain.Models;

/// <summary>
/// Defines the logical environment names used for lifecycle separation.
/// </summary>
public enum EnvironmentName
{
    /// <summary>
    /// Represents a production environment.
    /// </summary>
    Prod = 1,

    /// <summary>
    /// Represents a testing environment.
    /// </summary>
    Test = 2,

    /// <summary>
    /// Represents a development environment.
    /// </summary>
    Dev = 3
}
