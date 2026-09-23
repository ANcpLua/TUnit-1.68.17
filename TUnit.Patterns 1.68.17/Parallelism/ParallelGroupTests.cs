using System.Collections.Concurrent;

namespace TUnit.Patterns.Parallelism;

public static class Groups
{
    public const string Database = "Database";
    public const string Api = "Api";
}

/// <summary>Counts running tests per group; a test asks how many of the *other* group ran while it held the gate.</summary>
public static class GroupGate
{
    private const int Delay = 20;

    private static readonly ConcurrentDictionary<string, int> Running = new(StringComparer.Ordinal);

    public static async Task<int> HoldAsync(string group, string otherGroup)
    {
        Running.AddOrUpdate(group, 1, static (_, running) => running + 1);
        await Task.Delay(Delay);
        var overlap = Running.GetValueOrDefault(otherGroup);
        Running.AddOrUpdate(group, 0, static (_, running) => running - 1);
        return overlap;
    }
}

// Classes in the same [ParallelGroup] run in parallel with each other, never with another group.
[ParallelGroup(Groups.Database)]
public sealed class UserRepositoryTests
{
    [Test, Repeat(2)]
    public async Task Never_overlaps_the_api_group() => await Assert.That(await GroupGate.HoldAsync(Groups.Database, Groups.Api)).IsEqualTo(0);
}

[ParallelGroup(Groups.Database)]
public sealed class OrderRepositoryTests
{
    [Test, Repeat(2)]
    public async Task Never_overlaps_the_api_group() => await Assert.That(await GroupGate.HoldAsync(Groups.Database, Groups.Api)).IsEqualTo(0);
}

[ParallelGroup(Groups.Api)]
public sealed class PaymentApiTests
{
    [Test, Repeat(2)]
    public async Task Never_overlaps_the_database_group() => await Assert.That(await GroupGate.HoldAsync(Groups.Api, Groups.Database)).IsEqualTo(0);
}
