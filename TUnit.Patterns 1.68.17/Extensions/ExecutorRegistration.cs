using TUnit.Core.Interfaces;

namespace TUnit.Patterns.Extensions;

/// <summary>Four at a time: the default a <see cref="ThrottlingExecutor"/> brings along.</summary>
public sealed record FourAtATime : IParallelLimit
{
    public int Limit => 4;
}

/// <summary>Serial: an explicit, stricter limit a single test asks for.</summary>
public sealed record OneAtATime : IParallelLimit
{
    public int Limit => 1;
}

/// <summary>
/// A test executor that is also a registration receiver: once installed it supplies its own default parallel limit.
/// </summary>
public sealed class ThrottlingExecutor : ITestExecutor, ITestRegisteredEventReceiver
{
    public int Order => 0;

    public ValueTask OnTestRegistered(TestRegisteredContext context)
    {
        context.SetParallelLimiter(new FourAtATime());
        return default;
    }

    public ValueTask ExecuteTest(TestContext context, Func<ValueTask> action) => action();
}

/// <summary>Installs <see cref="ThrottlingExecutor"/> at registration time instead of via [TestExecutor&lt;T&gt;].</summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class ThrottledAttribute : Attribute, ITestRegisteredEventReceiver
{
    public int Order => 0;

    public ValueTask OnTestRegistered(TestRegisteredContext context)
    {
        context.SetTestExecutor(new ThrottlingExecutor());
        return default;
    }
}
