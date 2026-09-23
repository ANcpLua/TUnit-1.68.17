using TUnit.Core.Interfaces;

namespace TUnit.Patterns.Data;

public sealed class SumCases
{
    public static IEnumerable<(int Left, int Right, int Total)> All()
    {
        yield return (1, 2, 3);
        yield return (40, 2, 42);
    }
}

public sealed record Order(string Id, decimal Total);

/// <summary>TypedDataSourceAttribute&lt;T&gt;: the lowest-level strongly typed data source.</summary>
public sealed class SampleOrdersAttribute : TypedDataSourceAttribute<Order>
{
    public const string FirstId = "ORD-1";
    public const string SecondId = "ORD-2";

    public override async IAsyncEnumerable<Func<Task<Order>>> GetTypedDataRowsAsync(DataGeneratorMetadata dataGeneratorMetadata)
    {
        yield return static () => Task.FromResult(new Order(FirstId, 9.99m));
        yield return static () => Task.FromResult(new Order(SecondId, 0m));
        await Task.CompletedTask;
    }
}

/// <summary>IKeyedDataSource: TUnit sets Key before InitializeAsync, so a keyed fixture knows which key it serves.</summary>
public sealed class KeyedBroker : IAsyncInitializer, IKeyedDataSource
{
    public string Key { get; set; } = string.Empty;

    public string? KeyAtInitialization { get; private set; }

    public Task InitializeAsync()
    {
        KeyAtInitialization = Key;
        return Task.CompletedTask;
    }
}

public sealed class MoreDataSourceTests
{
    private const string Tenant = "tenant-a";

    [Test]
    [MethodDataSource<SumCases>(nameof(SumCases.All))]
    public async Task Generic_method_data_source(int left, int right, int total) => await Assert.That(left + right).IsEqualTo(total);

    [Test]
    [SampleOrders]
    public async Task Typed_data_source_yields_instances(Order order) =>
        await Assert.That(order.Id).IsIn([SampleOrdersAttribute.FirstId, SampleOrdersAttribute.SecondId]);

    [Test]
    [ClassDataSource<KeyedBroker>(Shared = SharedType.Keyed, Key = Tenant)]
    public async Task Keyed_fixture_knows_its_key(KeyedBroker broker)
    {
        await Assert.That(broker.Key).IsEqualTo(Tenant);
        await Assert.That(broker.KeyAtInitialization).IsEqualTo(Tenant);
    }
}
