using TUnit.Mocks;

namespace TUnit.Mocks_1._68._17;

public readonly ref struct Reading(int value)
{
    public int Value { get; } = value;
}

public delegate void FrameReceived(ReadOnlySpan<byte> frame);

public delegate void BudgetSpent(ref int remaining);

/// <summary>Stack-only event arguments: a ref struct payload, a span, and a by-reference counter.</summary>
public interface ISensor
{
    event EventHandler<Reading>? Measured;

    event FrameReceived? FrameReceived;

    event BudgetSpent? BudgetSpent;

    void Poll();
}

public class RefStructEventTests
{
    private const int FirstReading = 42;
    private const int PolledReading = 99;
    private const int Budget = 10;

    [Test]
    public async Task Ref_struct_payload_reaches_the_subscriber()
    {
        var sensor = ISensor.Mock();
        var received = 0;
        sensor.Object.Measured += (_, reading) => received = reading.Value;

        sensor.RaiseMeasured(new Reading(FirstReading));

        await Assert.That(received).IsEqualTo(FirstReading);
    }

    [Test]
    public async Task Span_argument_is_raised_without_copying_to_the_heap()
    {
        var sensor = ISensor.Mock();
        var length = 0;
        sensor.Object.FrameReceived += frame => length = frame.Length;

        sensor.RaiseFrameReceived(stackalloc byte[] { 1, 2, 3 });

        await Assert.That(length).IsEqualTo(3);
    }

    // ref modifiers survive: every subscriber sees, and changes, the caller's variable.
    [Test]
    public async Task Ref_argument_changes_reach_the_caller_and_later_subscribers()
    {
        var sensor = ISensor.Mock();
        sensor.Object.BudgetSpent += (ref int remaining) => remaining -= 3;
        sensor.Object.BudgetSpent += (ref int remaining) => remaining -= 4;
        var budget = Budget;

        sensor.RaiseBudgetSpent(ref budget);

        await Assert.That(budget).IsEqualTo(Budget - 3 - 4);
    }

    // No deferred .RaisesMeasured(...) exists for a stack-only argument - a setup cannot keep it alive -
    // so the documented replacement creates a fresh argument inside a callback when the call runs.
    [Test]
    public async Task Callback_creates_the_argument_when_the_setup_runs()
    {
        var sensor = ISensor.Mock();
        var received = 0;
        sensor.Object.Measured += (_, reading) => received = reading.Value;
        sensor.Poll().Callback(() => sensor.RaiseMeasured(new Reading(PolledReading)));

        sensor.Object.Poll();

        await Assert.That(received).IsEqualTo(PolledReading);
    }
}
