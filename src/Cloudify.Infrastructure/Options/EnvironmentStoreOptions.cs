namespace Cloudify.Infrastructure.Options;

/// <summary>
/// Represents configuration options for environment storage.
/// </summary>
public sealed class EnvironmentStoreOptions
{
    /// <summary>
    /// Gets the configuration section name for these options.
    /// </summary>
    public const string SectionName = "Cloudify:Storage";

    /// <summary>
    /// Gets or sets the storage provider name.
    /// </summary>
    public string Provider { get; set; } = "InMemory";
}
