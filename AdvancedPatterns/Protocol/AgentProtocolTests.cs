using System.Net;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.TestHost;

namespace AdvancedPatterns.Tests.Protocol;

public sealed class AgentProtocolTests
{
    [Test]
    public async Task RunEndpoint_ExecutesRegisteredAgentWithoutLiveServices(CancellationToken cancellationToken)
    {
        await using AgentProtocolHost host = await AgentProtocolHost.StartAsync(new ScriptedAgentRuntime(
            expectedAgent: "researcher",
            responseText: "deterministic answer"),
            cancellationToken);

        using HttpResponseMessage response = await host.Client.PostAsJsonAsync(
            "/agents/researcher/run",
            new AgentRunRequest("summarize the test strategy"),
            cancellationToken);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        AgentRunResponse? body = await response.Content.ReadFromJsonAsync<AgentRunResponse>(
            cancellationToken: cancellationToken);
        await Assert.That(body).IsNotNull();
        await Assert.That(body!.AgentName).IsEqualTo("researcher");
        await Assert.That(body.Text).IsEqualTo("deterministic answer");
    }

    [Test]
    public async Task RunEndpoint_ReturnsNotFoundForUnknownAgent(CancellationToken cancellationToken)
    {
        await using AgentProtocolHost host = await AgentProtocolHost.StartAsync(new ScriptedAgentRuntime(
            expectedAgent: "planner",
            responseText: "plan"),
            cancellationToken);

        using HttpResponseMessage response = await host.Client.PostAsJsonAsync(
            "/agents/writer/run",
            new AgentRunRequest("draft"),
            cancellationToken);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }
}

internal sealed class AgentProtocolHost : IAsyncDisposable
{
    private readonly WebApplication _app;

    private AgentProtocolHost(WebApplication app, HttpClient client)
    {
        _app = app;
        Client = client;
    }

    public HttpClient Client { get; }

    public static async Task<AgentProtocolHost> StartAsync(
        IAgentRuntime agentRuntime,
        CancellationToken cancellationToken)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();
        builder.Services.AddSingleton(agentRuntime);

        WebApplication app = builder.Build();

        app.MapPost("/agents/{agentName}/run", async (
            string agentName,
            AgentRunRequest request,
            IAgentRuntime runtime) =>
        {
            AgentRunResponse? response = await runtime.RunAsync(agentName, request.Input);
            return response is null ? Results.NotFound() : Results.Json(response);
        });

        await app.StartAsync(cancellationToken);

        TestServer server = app.Services.GetRequiredService<IServer>() as TestServer
            ?? throw new InvalidOperationException("Expected in-memory TestServer.");

        return new AgentProtocolHost(app, server.CreateClient());
    }

    public async ValueTask DisposeAsync()
    {
        Client.Dispose();
        await _app.DisposeAsync();
    }
}

internal interface IAgentRuntime
{
    Task<AgentRunResponse?> RunAsync(string agentName, string input);
}

internal sealed class ScriptedAgentRuntime(string expectedAgent, string responseText) : IAgentRuntime
{
    public Task<AgentRunResponse?> RunAsync(string agentName, string input)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(input);

        AgentRunResponse? response = string.Equals(agentName, expectedAgent, StringComparison.Ordinal)
            ? new AgentRunResponse(agentName, responseText)
            : null;

        return Task.FromResult(response);
    }
}

internal sealed record AgentRunRequest(string Input);

internal sealed record AgentRunResponse(string AgentName, string Text);
