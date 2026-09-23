namespace AdvancedPatterns.Tests.Contracts;

public interface IAsyncQueue
{
    ValueTask EnqueueAsync(string item);

    ValueTask<string?> DequeueAsync();

    ValueTask<int> CountAsync();
}

public interface IQueueFixture
{
    IAsyncQueue CreateQueue();
}

public abstract class QueueContractTests<TFixture>
    where TFixture : IQueueFixture, new()
{
    private readonly TFixture _fixture = new();

    [Test]
    public async Task DequeueAsync_ReturnsItemsInInsertionOrder()
    {
        IAsyncQueue queue = _fixture.CreateQueue();

        await queue.EnqueueAsync("first");
        await queue.EnqueueAsync("second");

        await Assert.That(await queue.DequeueAsync()).IsEqualTo("first");
        await Assert.That(await queue.DequeueAsync()).IsEqualTo("second");
        await Assert.That(await queue.DequeueAsync()).IsNull();
    }

    [Test]
    public async Task CountAsync_TracksEnqueuedAndDequeuedItems()
    {
        IAsyncQueue queue = _fixture.CreateQueue();

        await Assert.That(await queue.CountAsync()).IsEqualTo(0);

        await queue.EnqueueAsync("alpha");
        await queue.EnqueueAsync("beta");
        await Assert.That(await queue.CountAsync()).IsEqualTo(2);

        _ = await queue.DequeueAsync();
        await Assert.That(await queue.CountAsync()).IsEqualTo(1);
    }
}

[InheritsTests]
public sealed class InMemoryQueueContractTests : QueueContractTests<InMemoryQueueFixture>;

public sealed class InMemoryQueueFixture : IQueueFixture
{
    public IAsyncQueue CreateQueue() => new InMemoryQueue();
}

internal sealed class InMemoryQueue : IAsyncQueue
{
    private readonly Queue<string> _items = new();

    public ValueTask EnqueueAsync(string item)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(item);

        _items.Enqueue(item);
        return ValueTask.CompletedTask;
    }

    public ValueTask<string?> DequeueAsync() =>
        ValueTask.FromResult(_items.TryDequeue(out string? item) ? item : null);

    public ValueTask<int> CountAsync() => ValueTask.FromResult(_items.Count);
}
