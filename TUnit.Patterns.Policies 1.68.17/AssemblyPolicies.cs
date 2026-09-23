using TUnit.Core.Executors;
using TUnit.Patterns.Policies;

// Assembly-level policies change the behaviour of every test in the assembly, which is why they live in
// their own small project.
[assembly: System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
[assembly: NotInParallel]
[assembly: ParallelLimiter<AssemblyWide>]
[assembly: Retry(Policies.RetryLimit)]
[assembly: Timeout(Policies.TimeoutMs)]
[assembly: Culture(Policies.CultureName)]
[assembly: Category(Policies.CategoryName)]

namespace TUnit.Patterns.Policies;

/// <summary>
/// An assembly-level limiter controls concurrency across this project's tests.
/// </summary>
public sealed record AssemblyWide : TUnit.Core.Interfaces.IParallelLimit
{
    public int Limit => 4;
}

public static class Policies
{
    public const int RetryLimit = 2;
    public const int TimeoutMs = 10_000;
    public const string CultureName = "de-AT";
    public const string CategoryName = "Policy";
}
