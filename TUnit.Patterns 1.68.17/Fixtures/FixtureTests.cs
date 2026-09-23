namespace TUnit.Patterns.Fixtures;

public sealed class NestedInjectionTests
{
    [ClassDataSource<Application>(Shared = SharedType.PerClass)]
    public required Application Application { get; init; }

    [Test]
    public async Task Nested_property_is_initialized_before_its_owner()
    {
        await Assert.That(Application.ConfiguredEndpoint).IsNotEmpty();
        await Assert.That(Application.ConfiguredEndpoint).IsEqualTo(Application.Broker.Endpoint);
    }

    [Test]
    [ClassDataSource<Broker>(Shared = SharedType.Keyed, Key = Application.SharedKey)]
    public async Task Keyed_sharing_hands_out_the_same_instance(Broker broker) =>
        await Assert.That(broker).IsSameReferenceAs(Application.Broker);
}

public sealed class DiscoveryInitializerTests
{
    [ClassDataSource<CaseCatalogue>(Shared = SharedType.PerClass)]
    public required CaseCatalogue Catalogue { get; init; }

    public IEnumerable<string> Cases => Catalogue.Cases;

    [Test]
    [InstanceMethodDataSource(nameof(Cases))]
    public async Task Rows_come_from_a_discovery_initialized_fixture(string @case) =>
        await Assert.That(@case).IsNotEmpty();
}
