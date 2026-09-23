namespace TUnit.Patterns.Data;

/// <summary>Async, strongly typed data source: yields (index, value) pairs of the Fibonacci sequence.</summary>
public sealed class FibonacciAttribute(int count) : AsyncDataSourceGeneratorAttribute<int, long>
{
    protected override async IAsyncEnumerable<Func<Task<(int, long)>>> GenerateDataSourcesAsync(DataGeneratorMetadata dataGeneratorMetadata)
    {
        long previous = 0;
        long current = 1;

        for (var index = 0; index < count; index++)
        {
            var captured = (index, previous);
            yield return () => Task.FromResult(captured);

            (previous, current) = (current, previous + current);
            await Task.Yield();
        }
    }
}
