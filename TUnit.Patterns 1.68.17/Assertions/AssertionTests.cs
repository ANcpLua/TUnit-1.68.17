using TUnit.Assertions.Exceptions;

namespace TUnit.Patterns.Assertions;

public sealed class AssertionTests
{
    private const string OrderId = "ORD-1";
    private const string Line = "widget";
    private const string Prefix = "ORD-";
    private const string Other = "INV-";
    private const decimal Total = 9.99m;
    private const decimal Refund = -1m;

    private static readonly Order Payable = new(OrderId, Total, [Line]);
    private static readonly Order Empty = new(OrderId, Total, []);
    private static readonly Order Negative = new(OrderId, Refund, [Line]);

    [Test]
    public async Task Generated_assertions_chain()
    {
        await Assert.That(Payable)
            .IsPayable()
            .And.HasAtLeastLines(1);
    }

    [Test]
    public async Task Generated_assertion_reports_its_custom_failure()
    {
        var exception = await Assert.That(async () => await Assert.That(Empty).IsPayable()).Throws<AssertionException>();

        await Assert.That(exception!.Message).Contains(OrderAssertions.EmptyOrder);
    }

    [Test]
    public async Task Or_short_circuits_on_the_first_pass() =>
        await Assert.That(Negative)
            .IsPayable()
            .Or.HasAtLeastLines(1);

    [Test]
    public async Task Lifted_assertions_and_their_negation()
    {
        await Assert.That(OrderId).HasPrefix(Prefix);
        await Assert.That(OrderId).LacksPrefix(Other);
    }
}
