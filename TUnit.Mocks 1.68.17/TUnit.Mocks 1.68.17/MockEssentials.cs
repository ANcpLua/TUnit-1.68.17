using TUnit.Mocks;
using TUnit.Mocks.Exceptions;

namespace TUnit.Mocks_1._68._17;

public interface IUserDirectory
{
    Task<string?> FindEmailAsync(string userName);
}

public interface IMailer
{
    Task SendAsync(string to, string subject);
}

public sealed class WelcomeService(IUserDirectory directory, IMailer mailer)
{
    public async Task<bool> SendWelcomeAsync(string userName)
    {
        var email = await directory.FindEmailAsync(userName);

        if (email is null)
        {
            return false;
        }

        await mailer.SendAsync(email, "Welcome to TUnit!");

        return true;
    }
}

public class MockEssentials
{
    [Test]
    public async Task Known_user_gets_a_welcome_mail()
    {
        // T.Mock() is a C# 14 static extension — and the returned wrapper IS the
        // interface, so it passes straight into the constructor without .Object.
        var directory = IUserDirectory.Mock();
        var mailer = IMailer.Mock();
        directory.FindEmailAsync("ada").Returns("ada@example.org");

        var sent = await new WelcomeService(directory, mailer).SendWelcomeAsync("ada");

        await Assert.That(sent).IsTrue();
        mailer.SendAsync("ada@example.org", Any()).WasCalled(Times.Once);
    }

    [Test]
    public async Task Unknown_user_sends_nothing()
    {
        // Loose mocks answer unconfigured calls with smart defaults — FindEmailAsync
        // yields a completed Task<string?> holding null, no setup required.
        var directory = IUserDirectory.Mock();
        var mailer = IMailer.Mock();

        var sent = await new WelcomeService(directory, mailer).SendWelcomeAsync("ghost");

        await Assert.That(sent).IsFalse();
        mailer.SendAsync(Any(), Any()).WasNeverCalled();
    }

    [Test]
    public async Task Inline_lambdas_are_argument_matchers()
    {
        // Predicates go straight into the setup call — no Arg.Is<T>(...) ceremony.
        var directory = IUserDirectory.Mock();
        directory.FindEmailAsync(name => name!.StartsWith("a")).Returns("a@example.org");

        IUserDirectory lookup = directory;

        await Assert.That(await lookup.FindEmailAsync("ada")).IsEqualTo("a@example.org");
        await Assert.That(await lookup.FindEmailAsync("bob")).IsNull();
    }

    [Test]
    public async Task Captured_arguments_are_inspectable()
    {
        // Every matcher records the values it sees — keep it in a variable to inspect them.
        var mailer = IMailer.Mock();
        var to = Any<string>();
        mailer.SendAsync(to, Any());

        IMailer sender = mailer;
        await sender.SendAsync("first@example.org", "hi");
        await sender.SendAsync("second@example.org", "hi");

        await Assert.That(to.Values).Count().IsEqualTo(2);
        await Assert.That(to.Latest).IsEqualTo("second@example.org");
    }

    [Test]
    public async Task Sequential_setups_model_flaky_dependencies()
    {
        // .Then() gives each successive call its own behavior: fail once, then recover.
        var directory = IUserDirectory.Mock();
        directory.FindEmailAsync(Any())
            .Throws<TimeoutException>()
            .Then()
            .Returns("ada@example.org");

        IUserDirectory lookup = directory;

        await Assert.That(async () => await lookup.FindEmailAsync("ada")).Throws<TimeoutException>();
        await Assert.That(await lookup.FindEmailAsync("ada")).IsEqualTo("ada@example.org");
    }

    [Test]
    public async Task Strict_mocks_reject_surprise_calls()
    {
        var directory = IUserDirectory.Mock(MockBehavior.Strict);

        IUserDirectory lookup = directory;

        await Assert.That(async () => await lookup.FindEmailAsync("ada"))
            .Throws<MockStrictBehaviorException>();
    }
}
