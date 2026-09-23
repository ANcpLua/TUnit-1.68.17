namespace VendorSdk;

public static class QuotaGate
{
    /// <summary>Admits a client unless the feature bag carries a quota policy that rejects it.</summary>
    public static bool TryAdmit(IFeatureBag features, string clientId)
    {
        var policy = features.Get<IQuotaPolicy>();

        return policy is null || policy.Allow(clientId);
    }
}
