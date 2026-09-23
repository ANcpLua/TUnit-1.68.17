using TUnit.Mocks;

namespace TUnit.Mocks_1._68._17;

public interface IClock
{
    static abstract IClock System { get; }

    DateTimeOffset Now { get; }
}

public sealed class Scheduler(IClock clock)
{
    public bool CanRun() => clock.Now.Hour < 12;
}

public class StaticAbstractTests
{
    private static readonly DateTimeOffset Morning = new(2026, 8, 29, 9, 0, 0, TimeSpan.Zero);

    // Pass .Object to use the mocked instance members of an interface with static abstract members.
    [Test]
    public async Task Interfaces_with_static_abstract_members_are_mockable()
    {
        var clock = IClock.Mock();
        clock.Now.Returns(Morning);

        var canRun = new Scheduler(clock.Object).CanRun();

        await Assert.That(canRun).IsTrue();
    }
}
