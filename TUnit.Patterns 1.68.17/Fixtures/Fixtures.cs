using TUnit.Core.Interfaces;

namespace TUnit.Patterns.Fixtures;

/// <summary>Leaf resource: async start, async stop.</summary>
public sealed class Broker : IAsyncInitializer, IAsyncDisposable
{
    private const string EndpointFormat = "amqp://broker-{0}";

    private static int _instances;

    public string Endpoint { get; private set; } = string.Empty;

    public Task InitializeAsync()
    {
        Endpoint = string.Format(null, EndpointFormat, Interlocked.Increment(ref _instances));
        return Task.CompletedTask;
    }

    public ValueTask DisposeAsync() => default;
}

/// <summary>Composite: its own injected property is initialized before its InitializeAsync runs.</summary>
public sealed class Application : IAsyncInitializer, IAsyncDisposable
{
    public const string SharedKey = "integration";

    [ClassDataSource<Broker>(Shared = SharedType.Keyed, Key = SharedKey)]
    public required Broker Broker { get; init; }

    public string ConfiguredEndpoint { get; private set; } = string.Empty;

    public Task InitializeAsync()
    {
        ConfiguredEndpoint = Broker.Endpoint;
        return Task.CompletedTask;
    }

    public ValueTask DisposeAsync() => default;
}

/// <summary>Discovery-time initialization feeds an instance data source, so the rows exist when tests are enumerated.</summary>
public sealed class CaseCatalogue : IAsyncDiscoveryInitializer
{
    private const string First = "alpha";
    private const string Second = "beta";

    private readonly List<string> _cases = [];

    public IEnumerable<string> Cases => _cases;

    public Task InitializeAsync()
    {
        _cases.AddRange([First, Second]);
        return Task.CompletedTask;
    }
}
