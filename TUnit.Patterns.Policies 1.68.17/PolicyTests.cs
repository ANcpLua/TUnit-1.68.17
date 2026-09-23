using System.Globalization;

namespace TUnit.Patterns.Policies;

public sealed class PolicyTests
{
    private const int Delay = 20;
    private const decimal Value = 21.12m;
    private const string AustrianFormatted = "21,12";

    private static int _running;

    [Test, Repeat(2)]
    public async Task Assembly_wide_not_in_parallel_serializes_every_test()
    {
        var running = Interlocked.Increment(ref _running);
        await Task.Delay(Delay);
        Interlocked.Decrement(ref _running);

        await Assert.That(running).IsEqualTo(1);
    }

    [Test]
    public async Task Assembly_wide_retry_limit_is_visible_on_the_test_details() =>
        await Assert.That(TestContext.Current!.Metadata.TestDetails.RetryLimit).IsEqualTo(Policies.RetryLimit);

    [Test]
    public async Task Assembly_wide_timeout_is_visible_on_the_test_details() =>
        await Assert.That(TestContext.Current!.Metadata.TestDetails.Timeout).IsEqualTo(TimeSpan.FromMilliseconds(Policies.TimeoutMs));

    [Test]
    public async Task Assembly_wide_culture_applies_to_the_test_thread() =>
        await Assert.That(Value.ToString(CultureInfo.CurrentCulture)).IsEqualTo(AustrianFormatted);

    [Test]
    public async Task Assembly_wide_category_is_attached_to_every_test() =>
        await Assert.That(TestContext.Current!.Metadata.TestDetails.Categories).Contains(Policies.CategoryName);
}

public sealed class SecondClassTests
{
    private const int Delay = 20;

    private static int _running;

    // A second class proves the constraint spans classes, not only methods of one class.
    [Test, Repeat(2)]
    public async Task Also_serialized()
    {
        var running = Interlocked.Increment(ref _running);
        await Task.Delay(Delay);
        Interlocked.Decrement(ref _running);

        await Assert.That(running).IsEqualTo(1);
    }
}
