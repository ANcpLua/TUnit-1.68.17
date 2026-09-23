using System.Diagnostics;
using TUnit.Core.Enums;
using TUnit.Core.Interfaces;

namespace TUnit.Patterns.Extensions;

/// <summary>
/// Event receivers ride on attributes. EventReceiverStage.Early places the start receiver before
/// [Before(Test)] hooks and the end receiver before [After(Test)] hooks, so hooks on both sides can
/// read what the receiver recorded.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Method)]
public sealed class StopwatchAttribute : Attribute, ITestStartEventReceiver, ITestEndEventReceiver
{
    public const string StartedKey = "StopwatchStarted";
    public const string ElapsedKey = "StopwatchElapsed";

    public int Order => 0;

    public EventReceiverStage Stage => EventReceiverStage.Early;

    public ValueTask OnTestStart(TestContext context)
    {
        context.StateBag[StartedKey] = Stopwatch.GetTimestamp();
        return default;
    }

    public ValueTask OnTestEnd(TestContext context)
    {
        var started = (long)context.StateBag[StartedKey]!;
        context.StateBag[ElapsedKey] = Stopwatch.GetElapsedTime(started);
        return default;
    }
}
