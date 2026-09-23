using TUnit.Mocks;

namespace TUnit.Mocks_1._68._17;

public class PriceCalculator
{
    public virtual decimal Net(decimal gross) => Math.Round(gross / 1.19m, 2);
}

public class WrappedInstanceTests
{
    [Test]
    public async Task Wrapped_instance_forwards_calls_and_records_them()
    {
        // Mock.Wrap forwards unconfigured calls to the real instance and records them.
        var spy = Mock.Wrap(new PriceCalculator());

        await Assert.That(spy.Object.Net(119m)).IsEqualTo(100m);
        spy.Net(119m).WasCalled(Times.Once);
    }
}
