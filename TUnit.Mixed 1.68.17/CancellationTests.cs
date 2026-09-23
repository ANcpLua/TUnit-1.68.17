using TUnit.Core.Executors;
using TUnit.Core.Interfaces;

namespace TUnit.Mixed_1._68._17;

public sealed class PerTestCancellationShowcase
{
    // Opt-in because a successful demonstration deliberately finishes with
    // TUnit's Cancelled state, which makes an ordinary test run unsuccessful.
    [Test]
    [Explicit]
    [Timeout(5_000)]
    public async Task External_callback_cancels_only_this_test(CancellationToken cancellationToken)
    {
        // Capture Execution itself for callbacks on another thread; TestContext.Current
        // is not guaranteed to flow into an arbitrary external callback.
        var execution = TestContext.Current!.Execution;
        var cancellationRequest = Task.Run(execution.Cancel);

        try
        {
            // Cancel() is cooperative: the test body must observe a TUnit token.
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
        }
        finally
        {
            await cancellationRequest;
        }
    }
}

public sealed class BeforeHookLinkedCancellationShowcase
{
    private CancellationTokenSource _externalCancellation = null!;

    [Before(Test)]
    public void LinkExternalToken(TestContext context)
    {
        _externalCancellation = new CancellationTokenSource();
        context.Execution.AddLinkedCancellationToken(_externalCancellation.Token);
    }

    [Test]
    [Timeout(5_000)]
    public async Task Hook_link_reaches_injected_and_context_tokens(CancellationToken injectedToken)
    {
        var execution = TestContext.Current!.Execution;

        await _externalCancellation.CancelAsync();

        await Assert.That(injectedToken).IsCancellationRequested();
        await Assert.That(execution.CancellationToken).IsCancellationRequested();
    }

    [After(Test)]
    public void DisposeToken() => _externalCancellation.Dispose();
}

public sealed class ExecutorLinkedCancellationShowcase
{
    [Test]
    [TestExecutor<CancellingExecutor>]
    public async Task Executor_link_reaches_injected_token(CancellationToken cancellationToken)
    {
        await Assert.That(cancellationToken).IsCancellationRequested();
    }
}

public sealed class TimeoutDiagnosticsShowcase
{
    public const string Diagnostic = "database container still starting after 2 health checks";

    // Opt-in because it deliberately times out while adding context to the cancellation exception.
    [Test]
    [Explicit]
    [Timeout(100)]
    public async Task Cancellation_handler_diagnostics_reach_the_report(CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
        }
        catch (OperationCanceledException exception)
        {
            // e.g. Aspire collecting resource state before giving up
            await Task.Delay(50, CancellationToken.None);
            throw new OperationCanceledException(Diagnostic, exception, exception.CancellationToken);
        }
    }
}

public sealed class CancellingExecutor : ITestExecutor
{
    public async ValueTask ExecuteTest(TestContext context, Func<ValueTask> action)
    {
        using var cancellationSource = new CancellationTokenSource();
        context.Execution.AddLinkedCancellationToken(cancellationSource.Token);
        await cancellationSource.CancelAsync();

        await action();
    }
}
