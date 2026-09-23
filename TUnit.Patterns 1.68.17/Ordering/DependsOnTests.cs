namespace TUnit.Patterns.Ordering;

/// <summary>
/// [DependsOn] sequences tests without giving up parallelism elsewhere. The dependency's TestContext is
/// reachable through TestContext.Dependencies, so state can be handed over via its StateBag.
/// </summary>
public sealed class DependsOnTests
{
    private const string OrderIdKey = "OrderId";
    private const string OrderId = "order-4711";

    public static bool Shipped { get; private set; }

    [Test]
    public void CreateOrder() => TestContext.Current!.StateBag[OrderIdKey] = OrderId;

    [Test]
    [DependsOn(nameof(CreateOrder))]
    public async Task PayOrder()
    {
        var createOrder = TestContext.Current!.Dependencies.GetTests(nameof(CreateOrder)).Single();

        await Assert.That(createOrder.Execution.Result?.State).IsEqualTo(TestState.Passed);
        await Assert.That(createOrder.StateBag[OrderIdKey]).IsEqualTo(OrderId);
    }

    [Test]
    [DependsOn(nameof(CreateOrder))]
    [DependsOn(nameof(PayOrder))]
    public async Task ShipOrder()
    {
        var dependencies = TestContext.Current!.Dependencies;

        await Assert.That(dependencies.GetTests(nameof(PayOrder)).Single().Execution.Result?.State).IsEqualTo(TestState.Passed);
        await Assert.That(dependencies.GetTests(nameof(CreateOrder)).Single().StateBag[OrderIdKey]).IsEqualTo(OrderId);

        Shipped = true;
    }
}

/// <summary>The generic form declares a dependency on a test in another class.</summary>
public sealed class CrossClassDependsOnTests
{
    [Test]
    [DependsOn<DependsOnTests>(nameof(DependsOnTests.ShipOrder))]
    public async Task Runs_after_the_order_was_shipped() => await Assert.That(DependsOnTests.Shipped).IsTrue();
}

/// <summary>
/// ProceedOnFailure lets a dependent test start even though its dependency failed. Both tests are [Explicit]
/// because the demonstration needs a genuinely failing test; run them with a tree-node filter on this class.
/// </summary>
public sealed class ProceedOnFailureShowcase
{
    private const string Deliberate = "deliberate failure";

    [Test, Explicit]
    public void Fails_on_purpose() => throw new InvalidOperationException(Deliberate);

    [Test, Explicit]
    [DependsOn(nameof(Fails_on_purpose), ProceedOnFailure = true)]
    public async Task Still_runs_and_can_inspect_the_failure()
    {
        var dependency = TestContext.Current!.Dependencies.GetTests(nameof(Fails_on_purpose)).Single();

        await Assert.That(dependency.Execution.Result?.State).IsEqualTo(TestState.Failed);
        await Assert.That(dependency.Execution.Result?.Exception?.Message).IsEqualTo(Deliberate);
    }
}

/// <summary>[NotInParallel(Order = n)] runs the tests sharing a constraint key serially, in the given order.</summary>
public sealed class NotInParallelOrderTests
{
    private const string Pipeline = "Pipeline";

    private static readonly List<string> Executed = [];

    [Test]
    [NotInParallel(Pipeline, Order = 1)]
    public async Task Extract()
    {
        Executed.Add(nameof(Extract));

        await Assert.That(Executed).IsEquivalentTo([nameof(Extract)]);
    }

    [Test]
    [NotInParallel(Pipeline, Order = 2)]
    public async Task Transform()
    {
        Executed.Add(nameof(Transform));

        await Assert.That(Executed).IsEquivalentTo([nameof(Extract), nameof(Transform)]);
    }

    [Test]
    [NotInParallel(Pipeline, Order = 3)]
    public async Task Load()
    {
        Executed.Add(nameof(Load));

        await Assert.That(Executed).IsEquivalentTo([nameof(Extract), nameof(Transform), nameof(Load)]);
    }
}
