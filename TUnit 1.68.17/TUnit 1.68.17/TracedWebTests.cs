using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using TUnit.AspNetCore;
using TUnit.AspNetCore.Interception;

namespace TUnit_1._68._17;

/// <summary>
/// WebApplicationTest gives every test its own isolated factory on top of the shared one, plus the isolation
/// helpers (UniqueId, GetIsolatedName, GetIsolatedPrefix) and optional HTTP exchange capture.
/// </summary>
public sealed class TracedWebTests : WebApplicationTest<WebApplicationFactory, Program>
{
    private const string TracePath = "/trace";
    private const string TableBaseName = "todos";

    protected override void ConfigureTestOptions(WebApplicationTestOptions options) => options.EnableHttpExchangeCapture = true;

    [Test]
    public async Task Server_sees_the_test_trace_and_test_id()
    {
        var echo = await Factory.CreateClient().GetFromJsonAsync<TraceEcho>(TracePath);

        await Assert.That(echo!.HasTraceParent).IsTrue();
        await Assert.That(echo.TraceId).IsEqualTo(Activity.Current!.TraceId.ToString());
        await Assert.That(echo.TestId).IsEqualTo(TestContext.Current!.Id);
    }

    [Test]
    public async Task Server_side_logs_are_routed_into_this_test()
    {
        await Factory.CreateClient().GetAsync(TracePath);

        await Assert.That(TestContext.Current!.GetStandardOutput()).Contains(TraceEcho.LogMessage);
    }

    // Verified on 1.68.17: the middleware writes to the HttpExchangeCapture registered in the SUT's services; the
    // WebApplicationTest.HttpCapture property hands out a separate, always-empty store - resolve it from Services.
    [Test]
    public async Task Http_exchanges_are_captured_for_assertions()
    {
        var response = await Factory.CreateClient().GetAsync(TracePath);
        var capture = Services.GetRequiredService<HttpExchangeCapture>();

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(capture.Last!.Request.Path).IsEqualTo(TracePath);
        await Assert.That(capture.Last.Response.StatusCode).IsEqualTo(HttpStatusCode.OK);
    }

    [Test]
    public async Task Isolation_helpers_are_available_on_the_base_class()
    {
        await Assert.That(GetIsolatedName(TableBaseName)).Contains(UniqueId.ToString());
        await Assert.That(GetIsolatedPrefix()).Contains(UniqueId.ToString());
    }
}
