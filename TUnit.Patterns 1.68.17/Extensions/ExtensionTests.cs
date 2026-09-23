using System.Globalization;
using TUnit.Core.Executors;

namespace TUnit.Patterns.Extensions;

public sealed class TestExecutorTests
{
    private const decimal Value = 21.12m;
    private const string AustrianFormatted = "21,12";

    [Test]
    [TestExecutor<ScopedCultureExecutor>]
    public async Task Executor_establishes_the_ambient_culture() =>
        await Assert.That(Value.ToString(CultureInfo.CurrentCulture)).IsEqualTo(AustrianFormatted);

    // The built-in [Culture] attribute is the same mechanism, shipped with TUnit.
    [Test, Culture(ScopedCultureExecutor.CultureName)]
    public async Task Built_in_culture_attribute_does_the_same() =>
        await Assert.That(Value.ToString(CultureInfo.CurrentCulture)).IsEqualTo(AustrianFormatted);
}

[Stopwatch]
public sealed class EventReceiverTests
{
    private const string HookKey = "HookSawStopwatch";

    [Before(Test)]
    public void Hook_runs_after_early_receiver(TestContext context) =>
        context.StateBag[HookKey] = context.StateBag.ContainsKey(StopwatchAttribute.StartedKey);

    [Test]
    public async Task Early_stage_receiver_precedes_before_test_hooks()
    {
        var stateBag = TestContext.Current!.StateBag;

        await Assert.That((bool)stateBag[HookKey]!).IsTrue();
        await Assert.That(stateBag.ContainsKey(StopwatchAttribute.ElapsedKey)).IsFalse();
    }

    [After(Test)]
    public async Task Elapsed_is_already_recorded_in_after_test_hooks(TestContext context) =>
        await Assert.That((TimeSpan)context.StateBag[StopwatchAttribute.ElapsedKey]!).IsGreaterThan(TimeSpan.Zero);
}

public sealed class SkipTests
{
    private const string Windows = "WINDOWS";
    private const string Never = "NEVER";

    [Test, SkipOn(Windows)]
    public async Task Runs_everywhere_but_windows() =>
        await Assert.That(OperatingSystem.IsWindows()).IsFalse();

    [Test, SkipOn(Never)]
    public async Task Unknown_platform_never_skips() =>
        await Assert.That(TestContext.Current!.Execution.Result).IsNull();
}
