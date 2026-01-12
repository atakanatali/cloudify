namespace Cloudify.Infrastructure.Options;

/// <summary>
/// Represents configuration options for Docker Compose orchestration.
/// </summary>
public sealed class DockerComposeOptions
{
    /// <summary>
    /// Gets the configuration section name for these options.
    /// </summary>
    public const string SectionName = "Cloudify:DockerCompose";

    /// <summary>
    /// Gets or sets the Docker CLI command.
    /// </summary>
    public string DockerComposeCommand { get; set; } = "docker";

    /// <summary>
    /// Gets or sets the compose subcommand.
    /// </summary>
    public string ComposeSubcommand { get; set; } = "compose";

    /// <summary>
    /// Gets or sets the base working directory for environment compose files.
    /// </summary>
    public string WorkingDirectoryBase { get; set; } = "./data/environments";

    /// <summary>
    /// Gets or sets the command timeout in seconds.
    /// </summary>
    public int CommandTimeoutSeconds { get; set; } = 300;

    /// <summary>
    /// Gets or sets a value indicating whether to enable dry-run mode.
    /// </summary>
    public bool EnableDryRun { get; set; }
}
