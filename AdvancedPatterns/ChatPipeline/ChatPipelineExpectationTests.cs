using TUnit.Assertions.Enums;

namespace AdvancedPatterns.Tests.ChatPipeline;

public sealed class ChatPipelineExpectationTests
{
    [Test]
    public async Task RunAsync_SendsPersistedHistoryAndCapturesEveryServiceCall()
    {
        ChatPipelineHarness harness = new(
            new ServiceCallExpectation(
                ResponseText: "Paris.",
                VerifyInput: async messages => await Assert.That(messages.Select(m => m.Text)).IsEquivalentTo(
                    new[] { "What is the capital of France?" },
                    CollectionOrdering.Matching)),
            new ServiceCallExpectation(
                ResponseText: "Vienna.",
                VerifyInput: async messages => await Assert.That(messages.Select(m => m.Text)).IsEquivalentTo(
                    new[] { "What is the capital of France?", "Paris.", "And Austria?" },
                    CollectionOrdering.Matching)));

        ChatPipeline pipeline = harness.CreatePipeline();

        ChatMessage first = await pipeline.RunAsync("What is the capital of France?");
        ChatMessage second = await pipeline.RunAsync("And Austria?");

        await Assert.That(first.Text).IsEqualTo("Paris.");
        await Assert.That(second.Text).IsEqualTo("Vienna.");
        await Assert.That(harness.TotalServiceCalls).IsEqualTo(2);
        await Assert.That(pipeline.History.Select(m => m.Text)).IsEquivalentTo(
            new[] { "What is the capital of France?", "Paris.", "And Austria?", "Vienna." },
            CollectionOrdering.Matching);
    }

    [Test]
    public async Task RunAsync_FailsWhenServiceReceivesUnexpectedExtraCall()
    {
        ChatPipelineHarness harness = new(new ServiceCallExpectation("done"));
        ChatPipeline pipeline = harness.CreatePipeline();

        _ = await pipeline.RunAsync("first");

        await Assert.That(async () => { _ = await pipeline.RunAsync("second"); })
            .Throws<InvalidOperationException>()
            .WithMessageContaining("unexpected service call #2", StringComparison.OrdinalIgnoreCase);
    }
}

internal sealed class ChatPipelineHarness(params ServiceCallExpectation[] expectations)
{
    private readonly ScriptedChatService _service = new(expectations);

    public int TotalServiceCalls => _service.TotalCalls;

    public ChatPipeline CreatePipeline() => new(_service);
}

internal sealed class ChatPipeline(IChatService service)
{
    private readonly List<ChatMessage> _history = [];

    public IReadOnlyList<ChatMessage> History => _history;

    public async Task<ChatMessage> RunAsync(string userText)
    {
        _history.Add(new ChatMessage(ChatRole.User, userText));

        ChatMessage response = await service.GetResponseAsync(_history);
        _history.Add(response);

        return response;
    }
}

internal interface IChatService
{
    Task<ChatMessage> GetResponseAsync(IReadOnlyList<ChatMessage> messages);
}

internal sealed class ScriptedChatService(IReadOnlyList<ServiceCallExpectation> expectations) : IChatService
{
    private int _callIndex;

    public int TotalCalls => _callIndex;

    public async Task<ChatMessage> GetResponseAsync(IReadOnlyList<ChatMessage> messages)
    {
        int callNumber = _callIndex + 1;
        if (_callIndex >= expectations.Count)
        {
            throw new InvalidOperationException(
                $"Received unexpected service call #{callNumber}; only {expectations.Count} call(s) were expected.");
        }

        ServiceCallExpectation expectation = expectations[_callIndex++];
        if (expectation.VerifyInput is not null)
        {
            await expectation.VerifyInput(messages);
        }

        return new ChatMessage(ChatRole.Assistant, expectation.ResponseText);
    }
}

internal sealed record ServiceCallExpectation(
    string ResponseText,
    Func<IReadOnlyList<ChatMessage>, Task>? VerifyInput = null);

internal sealed record ChatMessage(ChatRole Role, string Text);

internal enum ChatRole
{
    User,
    Assistant
}
