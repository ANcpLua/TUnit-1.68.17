namespace TUnit.Patterns.Retry;

/// <summary>
/// Every attempt re-instantiates the class, but the TestContext persists: Execution.CurrentRetryAttempt is
/// the zero-based attempt number and Execution.RetryAttempts keeps the results of the failed attempts.
/// </summary>
public sealed class RetryTests
{
    private const int MaxRetries = 3;
    private const int PassOnAttempt = 2;
    private const int Backoff = 1;
    private const string StillWarmingUp = "still warming up";

    [Test]
    [RetryOnTransient(MaxRetries)]
    public async Task Custom_policy_retries_transient_failures_until_they_pass()
    {
        var execution = TestContext.Current!.Execution;

        if (execution.CurrentRetryAttempt < PassOnAttempt)
        {
            throw new TransientException(StillWarmingUp);
        }

        await Assert.That(execution.CurrentRetryAttempt).IsEqualTo(PassOnAttempt);
        await Assert.That(execution.RetryAttempts).Count().IsEqualTo(PassOnAttempt);
        await Assert.That(execution.RetryAttempts!.Select(static attempt => attempt.State)).All().Satisfy(static state => state.IsEqualTo(TestState.Failed));
    }

    // The built-in policy can filter by exception type and back off between attempts without a subclass.
    [Test]
    [Retry(MaxRetries, RetryOnExceptionTypes = [typeof(TransientException)], BackoffMs = Backoff)]
    public async Task Built_in_policy_filters_by_exception_type()
    {
        var execution = TestContext.Current!.Execution;

        if (execution.CurrentRetryAttempt < PassOnAttempt)
        {
            throw new TransientException(StillWarmingUp);
        }

        await Assert.That(execution.RetryAttempts!.Select(static attempt => attempt.Exception)).All().Satisfy(static exception => exception.IsTypeOf<TransientException>());
    }

    [Test]
    [RetryOnTransient(MaxRetries)]
    public async Task A_passing_first_attempt_has_no_retry_history()
    {
        var execution = TestContext.Current!.Execution;

        await Assert.That(execution.CurrentRetryAttempt).IsEqualTo(0);
        await Assert.That(execution.RetryAttempts).IsEmpty();
    }
}
