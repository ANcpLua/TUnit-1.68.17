using TUnit.Core.Interfaces;

namespace TUnit.Patterns.Parallelism;

/// <summary>Shared by every test that references it: at most <see cref="Limit"/> of them run concurrently.</summary>
public sealed record TwoAtATime : IParallelLimit
{
    public int Limit => 2;
}

[ParallelLimiter<TwoAtATime>]
public sealed class ParallelLimiterTests
{
    private const int Repeats = 9;
    private const int Delay = 20;

    private static int _running;
    private static int _peak;

    [Test]
    [Repeat(Repeats)]
    public async Task Concurrency_never_exceeds_the_limit()
    {
        var running = Interlocked.Increment(ref _running);
        InterlockedMax(ref _peak, running);

        await Task.Delay(Delay);

        Interlocked.Decrement(ref _running);

        await Assert.That(running).IsLessThanOrEqualTo(new TwoAtATime().Limit);
        await Assert.That(_peak).IsLessThanOrEqualTo(new TwoAtATime().Limit);
    }

    private static void InterlockedMax(ref int location, int value)
    {
        int current;
        do
        {
            current = location;
        }
        while (value > current && Interlocked.CompareExchange(ref location, value, current) != current);
    }
}

/// <summary>Tests sharing a [NotInParallel] key never overlap; a different key is free to run alongside them.</summary>
public sealed class NotInParallelKeyTests
{
    private const string SharedFile = "SharedFile";
    private const int Delay = 20;

    private static int _holders;

    [Test]
    [NotInParallel(SharedFile)]
    [Repeat(3)]
    public async Task Holds_the_file_alone()
    {
        var holders = Interlocked.Increment(ref _holders);
        await Task.Delay(Delay);
        Interlocked.Decrement(ref _holders);

        await Assert.That(holders).IsEqualTo(1);
    }
}
