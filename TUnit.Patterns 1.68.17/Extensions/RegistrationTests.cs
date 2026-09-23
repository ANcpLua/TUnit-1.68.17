using TUnit.Core.Exceptions;
using TUnit.Core.Executors;

namespace TUnit.Patterns.Extensions;

[Dispatch]
[AssignTestIdentifiers]
[LastTestObserver]
public sealed class RegistrationTests
{
    [Before(Test)]
    [HookExecutor<RecordingHookExecutor>]
    public void Setup(TestContext context)
    {
    }

    [Test]
    public async Task Hook_ran_through_the_custom_executor() =>
        await Assert.That(RecordingHookExecutor.ExecutedHooks).Contains(nameof(Setup));

    [Test]
    public async Task Registration_receiver_installed_the_hook_executor() =>
        await Assert.That(TestContext.Current!.Execution.CustomHookExecutor).IsTypeOf<RecordingHookExecutor>();

    [Test]
    public async Task Discovery_receiver_assigned_an_id() =>
        await Assert.That((int)TestContext.Current!.StateBag[AssignTestIdentifiersAttribute.TestIdKey]!).IsGreaterThan(0);

    [Test]
    [BracketedName]
    public async Task Formatter_rewrites_the_display_name() =>
        await Assert.That(TestContext.Current!.Metadata.DisplayName)
            .IsEqualTo(string.Concat(BracketedNameAttribute.Prefix, nameof(Formatter_rewrites_the_display_name), BracketedNameAttribute.Suffix));

    [After(Class)]
    public static async Task Last_test_in_class_was_observed(ClassHookContext context) =>
        await Assert.That(LastTestObserverAttribute.LastInClassSeen).IsTrue();
}

public sealed class SkipAndInconclusiveTests
{
    private const string Reason = "not applicable here";
    private const string EnvironmentKey = "environment";
    private const string Staging = "staging";

    [Test]
    public async Task Conditional_skips_that_do_not_trigger_keep_the_test_running()
    {
        Skip.When(false, Reason);
        Skip.Unless(true, Reason);

        await Assert.That(TestContext.Current!.Execution.SkipReason).IsNull();
    }

    // Reported as skipped, not failed.
    [Test]
    public void Skip_test_skips_at_run_time() => Skip.Test(Reason);

    // Opt-in: the pinned runner reports InconclusiveTestException as failed.
    [Test, Explicit]
    public void Inconclusive_at_run_time() => throw new InconclusiveTestException(Reason);

    // `--test-parameter environment=staging` makes the configured branch observable.
    [Test]
    public async Task Test_parameters_come_from_the_command_line()
    {
        var configured = TestContext.Parameters.TryGetValue(EnvironmentKey, out var values);

        await Assert.That(!configured || values!.Single() == Staging).IsTrue();
    }
}

public sealed class TimeoutRetryTests
{
    private const int TimeoutMs = 200;

    // Each retry attempt gets a fresh timeout; the first attempt deliberately runs into it.
    [Test]
    [Timeout(TimeoutMs)]
    [Retry(1)]
    public async Task First_attempt_times_out_then_the_retry_passes(CancellationToken cancellationToken)
    {
        var execution = TestContext.Current!.Execution;

        if (execution.CurrentRetryAttempt == 0)
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
        }

        await Assert.That(execution.CurrentRetryAttempt).IsEqualTo(1);
        await Assert.That(execution.RetryAttempts).Count().IsEqualTo(1);
    }
}
