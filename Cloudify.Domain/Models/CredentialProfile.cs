namespace Cloudify.Domain.Models;

/// <summary>
/// Represents credential settings for a resource.
/// </summary>
public sealed class CredentialProfile
{
    /// <summary>
    /// Gets the credential username.
    /// </summary>
    public string Username { get; }

    /// <summary>
    /// Gets the credential password.
    /// </summary>
    public string Password { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CredentialProfile"/> class.
    /// </summary>
    /// <param name="username">The credential username.</param>
    /// <param name="password">The credential password.</param>
    /// <exception cref="ArgumentException">Thrown when the username or password is empty.</exception>
    public CredentialProfile(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            throw new ArgumentException("Username is required.", nameof(username));
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("Password is required.", nameof(password));
        }

        Username = username;
        Password = password;
    }
}
