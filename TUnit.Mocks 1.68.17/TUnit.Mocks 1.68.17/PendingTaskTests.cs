namespace TUnit.Mocks_1._68._17;

public interface IQuoteFeed
{
    Task<decimal> LatestAsync(string symbol);
}

public class PendingTaskTests
{
    // An async factory keeps the call pending until the test supplies its result.
    [Test]
    public async Task Caller_timeout_beats_a_hanging_feed()
    {
        var feed = IQuoteFeed.Mock();
        var stall = new TaskCompletionSource<decimal>();
        feed.LatestAsync(Any()).Returns(async () => await stall.Task);

        IQuoteFeed quotes = feed;
        var pending = quotes.LatestAsync("TUNIT");
        var winner = await Task.WhenAny(pending, Task.Delay(TimeSpan.FromMilliseconds(50)));

        await Assert.That(winner).IsNotSameReferenceAs(pending);
        await Assert.That(pending.IsCompleted).IsFalse();

        stall.SetResult(99.5m);
        await Assert.That(await pending).IsEqualTo(99.5m);
    }

    [Test]
    public async Task Async_factory_runs_once_per_call()
    {
        var feed = IQuoteFeed.Mock();
        var calls = 0;
        feed.LatestAsync(Any()).Returns(async () =>
        {
            calls++;
            await Task.Yield();
            return 10m * calls;
        });

        IQuoteFeed quotes = feed;

        await Assert.That(await quotes.LatestAsync("A")).IsEqualTo(10m);
        await Assert.That(await quotes.LatestAsync("B")).IsEqualTo(20m);
    }
}
