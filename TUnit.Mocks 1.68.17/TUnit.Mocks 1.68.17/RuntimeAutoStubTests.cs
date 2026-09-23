using VendorSdk;

namespace TUnit.Mocks_1._68._17;

public class RuntimeAutoStubTests
{
    // VendorSdk requests an internal interface with no generated mock. Loose mode supplies
    // a runtime stub using the SDK's DynamicProxyGenAssembly2 internals grant.
    [Test]
    public async Task Sdk_internal_feature_lookup_gets_a_working_stub()
    {
        var features = IFeatureBag.Mock();

        var endpoint = await TelemetryPipeline.FlushAsync(features);

        await Assert.That(endpoint).IsEmpty();
    }
}
