using TUnit.Assertions.Exceptions;

namespace TUnit.Patterns.Assertions;

public sealed class MultipleTests
{
    [Test]
    public async Task Multiple_assertions_report_all_invalid_order_fields()
    {
        var order = new Order("ORD-1", -1m, []);

        var exception = await Assert.That(async () =>
        {
            using (Assert.Multiple())
            {
                await Assert.That(order.Total).IsGreaterThan(0m);
                await Assert.That(order.Lines).IsNotEmpty();
            }
        }).Throws<AssertionException>();

        var failures = ((AggregateException)exception!.InnerException!).InnerExceptions;
        await Assert.That(failures).Count().IsEqualTo(2);
    }
}
