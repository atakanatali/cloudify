namespace Cloudify.Domain.Models;

/// <summary>
/// Represents a declared port exposure policy for a resource.
/// </summary>
public sealed class PortPolicy
{
    private readonly List<int> _ports;

    /// <summary>
    /// Gets the declared exposed ports.
    /// </summary>
    public IReadOnlyCollection<int> ExposedPorts => _ports;

    /// <summary>
    /// Initializes a new instance of the <see cref="PortPolicy"/> class.
    /// </summary>
    /// <param name="ports">The ports to expose.</param>
    /// <exception cref="ArgumentException">Thrown when ports are invalid.</exception>
    public PortPolicy(IEnumerable<int> ports)
    {
        if (ports is null)
        {
            throw new ArgumentNullException(nameof(ports));
        }

        _ports = new List<int>();

        foreach (var port in ports)
        {
            if (port is < 1 or > 65535)
            {
                throw new ArgumentException("Ports must be between 1 and 65535.", nameof(ports));
            }

            _ports.Add(port);
        }
    }
}
