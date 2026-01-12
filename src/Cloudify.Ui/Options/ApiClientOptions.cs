namespace Cloudify.Ui.Options;

/// <summary>
/// Represents configuration options for connecting to the Cloudify API.
/// </summary>
public sealed class ApiClientOptions
{
    /// <summary>
    /// Gets the configuration section name for these options.
    /// </summary>
    public const string SectionName = "Cloudify:Api";

    /// <summary>
    /// Gets or sets the base URL for the Cloudify API.
    /// </summary>
    public string BaseUrl { get; set; } = "https://localhost:5001/";
}
