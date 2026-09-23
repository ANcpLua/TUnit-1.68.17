namespace TUnit.Patterns.Extensions;

[Throttled]
public sealed class ExecutorRegistrationTests
{
    [Test]
    public async Task Registered_executor_supplies_the_default_limiter()
    {
        var context = TestContext.Current!;

        await Assert.That(context.Parallelism.Limiter).IsTypeOf<FourAtATime>();
    }

    // A method can override the parallel limit supplied by its executor.
    [Test]
    [ParallelLimiter<OneAtATime>]
    public async Task Explicit_limiter_beats_the_executor_default()
    {
        var context = TestContext.Current!;

        await Assert.That(context.Parallelism.Limiter).IsTypeOf<OneAtATime>();
    }
}
