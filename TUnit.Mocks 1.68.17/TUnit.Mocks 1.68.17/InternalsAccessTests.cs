using TUnit.Mocks;
using VendorSdk;

namespace TUnit.Mocks_1._68._17;

public class InternalsAccessTests
{
    // Experimental internals access makes VendorSdk's IQuotaPolicy available for setup
    // through the opt-in properties and assembly item in this project's csproj.
    [Test]
    public async Task Internal_sdk_policy_is_fully_mockable()
    {
        var policy = IQuotaPolicy.Mock();
        policy.Allow(Any()).Returns(false);

        var features = IFeatureBag.Mock();
        features.Get<IQuotaPolicy>().Returns(policy.Object);

        var admitted = QuotaGate.TryAdmit(features, "tenant-42");

        await Assert.That(admitted).IsFalse();
        policy.Allow("tenant-42").WasCalled(Times.Once);
    }

    [Test]
    public async Task Explicit_null_overrides_auto_mocking()
    {
        // Loose mocks auto-mock interface returns once a generated factory exists (naming
        // IQuotaPolicy.Mock() above created one) — handing the SDK a genuine "no policy
        // registered" answer therefore takes an explicit null setup.
        var features = IFeatureBag.Mock();
        features.Get<IQuotaPolicy>().Returns((IQuotaPolicy?)null);

        await Assert.That(QuotaGate.TryAdmit(features, "anyone")).IsTrue();
    }
}
