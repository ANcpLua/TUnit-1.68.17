namespace VendorSdk;

public static class TelemetryPipeline
{
    /// <summary>
    ///     Flushes the telemetry channel resolved from the feature bag. Like real SDK code,
    ///     it assumes the feature exists — a null from <see cref="IFeatureBag.Get{T}" /> is an
    ///     immediate <see cref="NullReferenceException" /> deep inside the SDK.
    /// </summary>
    public static async Task<string> FlushAsync(IFeatureBag features)
    {
        var channel = features.Get<ITelemetryChannel>();

        await channel!.FlushAsync();

        return channel.Endpoint;
    }
}
