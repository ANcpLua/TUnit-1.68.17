namespace TUnit.Patterns.Dynamic;

/// <summary>Tests built in code: the lambda is an expression describing which method to call; arguments come from TestMethodArguments.</summary>
public sealed class DynamicTests
{
    private const string Greeting = "Hello";
    private const string Name = "TUnit";
    private const int Repeats = 2;

    public async Task Greets(string greeting, string name) =>
        await Assert.That(string.Concat(greeting, name)).StartsWith(greeting).And.EndsWith(name);

    [DynamicTestBuilder]
    public void Build(DynamicTestBuilderContext context) =>
        context.AddTest(new DynamicTest<DynamicTests>
        {
            TestMethod = tests => tests.Greets(DynamicTestHelper.Argument<string>(), DynamicTestHelper.Argument<string>()),
            TestMethodArguments = [Greeting, Name],
            Attributes = [new RepeatAttribute(Repeats)],
        });
}
