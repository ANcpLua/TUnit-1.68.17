namespace TUnit.Patterns.Retry;

/// <summary>Custom retry policy: only <see cref="TransientException"/> earns another attempt; anything else fails immediately.</summary>
public sealed class RetryOnTransientAttribute(int times) : RetryAttribute(times)
{
    public override Task<bool> ShouldRetry(TestContext context, Exception exception, int currentRetryCount) =>
        Task.FromResult(exception is TransientException);
}

public sealed class TransientException(string message) : Exception(message);
