using System.Collections.Concurrent;
using TUnit.Core.Interfaces;

namespace TUnit.Patterns.Extensions;

/// <summary>IHookExecutor: wraps every hook kind; here it records which hook methods ran through it.</summary>
public sealed class RecordingHookExecutor : IHookExecutor
{
    private static readonly ConcurrentQueue<string> Executed = new();

    public static IReadOnlyCollection<string> ExecutedHooks => [.. Executed];

    public ValueTask ExecuteBeforeTestDiscoveryHook(MethodMetadata hookMethodInfo, BeforeTestDiscoveryContext context, Func<ValueTask> action) => Run(hookMethodInfo, action);

    public ValueTask ExecuteBeforeTestSessionHook(MethodMetadata hookMethodInfo, TestSessionContext context, Func<ValueTask> action) => Run(hookMethodInfo, action);

    public ValueTask ExecuteBeforeAssemblyHook(MethodMetadata hookMethodInfo, AssemblyHookContext context, Func<ValueTask> action) => Run(hookMethodInfo, action);

    public ValueTask ExecuteBeforeClassHook(MethodMetadata hookMethodInfo, ClassHookContext context, Func<ValueTask> action) => Run(hookMethodInfo, action);

    public ValueTask ExecuteBeforeTestHook(MethodMetadata hookMethodInfo, TestContext context, Func<ValueTask> action) => Run(hookMethodInfo, action);

    public ValueTask ExecuteAfterTestDiscoveryHook(MethodMetadata hookMethodInfo, TestDiscoveryContext context, Func<ValueTask> action) => Run(hookMethodInfo, action);

    public ValueTask ExecuteAfterTestSessionHook(MethodMetadata hookMethodInfo, TestSessionContext context, Func<ValueTask> action) => Run(hookMethodInfo, action);

    public ValueTask ExecuteAfterAssemblyHook(MethodMetadata hookMethodInfo, AssemblyHookContext context, Func<ValueTask> action) => Run(hookMethodInfo, action);

    public ValueTask ExecuteAfterClassHook(MethodMetadata hookMethodInfo, ClassHookContext context, Func<ValueTask> action) => Run(hookMethodInfo, action);

    public ValueTask ExecuteAfterTestHook(MethodMetadata hookMethodInfo, TestContext context, Func<ValueTask> action) => Run(hookMethodInfo, action);

    private static async ValueTask Run(MethodMetadata hook, Func<ValueTask> action)
    {
        Executed.Enqueue(hook.Name);
        await action();
    }
}
