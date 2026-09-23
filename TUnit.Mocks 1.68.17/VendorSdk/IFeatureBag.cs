namespace VendorSdk;

/// <summary>
///     The SDK's extension point — hosts register capabilities, SDK internals resolve them.
///     Mirrors the shape of Azure Functions Worker's <c>IInvocationFeatures</c>.
/// </summary>
public interface IFeatureBag
{
    T? Get<T>() where T : class;
}
