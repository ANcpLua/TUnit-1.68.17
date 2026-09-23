using TUnit.Core.Interfaces;

namespace TUnit.Patterns.Extensions;

/// <summary>ITestRegisteredEventReceiver: rewrites how a test runs at registration time (hook executor, parallel limit).</summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class DispatchAttribute : Attribute, ITestRegisteredEventReceiver
{
    public int Order => 0;

    public ValueTask OnTestRegistered(TestRegisteredContext context)
    {
        context.SetHookExecutor(new RecordingHookExecutor());
        context.SetParallelLimiter(new Parallelism.TwoAtATime());
        return default;
    }
}

/// <summary>ITestDiscoveryEventReceiver: assigns a discovery-ordered id to every test it decorates (docs: "Global Test IDs").</summary>
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Method)]
public sealed class AssignTestIdentifiersAttribute : Attribute, ITestDiscoveryEventReceiver
{
    public const string TestIdKey = "TestId";

    private static int _nextId;

    public int Order => 0;

    public ValueTask OnTestDiscovered(DiscoveredTestContext discoveredTestContext)
    {
        discoveredTestContext.TestContext.StateBag[TestIdKey] = Interlocked.Increment(ref _nextId);
        return default;
    }
}

/// <summary>Last-test receivers fire once when the final test of a class / assembly / session finishes.</summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class LastTestObserverAttribute : Attribute, ILastTestInClassEventReceiver, ILastTestInAssemblyEventReceiver, ILastTestInTestSessionEventReceiver
{
    public static bool LastInClassSeen { get; private set; }

    public static bool LastInAssemblySeen { get; private set; }

    public static bool LastInSessionSeen { get; private set; }

    public int Order => 0;

    public ValueTask OnLastTestInClass(ClassHookContext context, TestContext testContext)
    {
        LastInClassSeen = true;
        return default;
    }

    public ValueTask OnLastTestInAssembly(AssemblyHookContext context, TestContext testContext)
    {
        LastInAssemblySeen = true;
        return default;
    }

    public ValueTask OnLastTestInTestSession(TestSessionContext context, TestContext testContext)
    {
        LastInSessionSeen = true;
        return default;
    }
}

/// <summary>DisplayNameFormatterAttribute: full control over the display name at discovery time.</summary>
public sealed class BracketedNameAttribute : DisplayNameFormatterAttribute
{
    public const string Prefix = "[";
    public const string Suffix = "]";

    protected override string FormatDisplayName(DiscoveredTestContext context) =>
        string.Concat(Prefix, context.TestContext.Metadata.TestName, Suffix);
}
