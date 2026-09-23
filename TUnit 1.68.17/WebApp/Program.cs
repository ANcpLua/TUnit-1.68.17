using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.UseHttpsRedirection();

app.MapGet("/ping", () => "Hello, World!");

// Echoes what the server sees of the caller's trace context; the test asserts it matches its own.
app.MapGet("/trace", (HttpContext http, ILogger<Program> logger) =>
{
    logger.LogInformation(TraceEcho.LogMessage);

    return new TraceEcho(
        Activity.Current?.TraceId.ToString(),
        http.Request.Headers[TraceEcho.TestIdHeader].ToString(),
        http.Request.Headers.ContainsKey(TraceEcho.TraceParentHeader));
});

app.Run();

public partial class Program;

public sealed record TraceEcho(string? TraceId, string TestId, bool HasTraceParent)
{
    public const string TestIdHeader = "X-TUnit-TestId";
    public const string TraceParentHeader = "traceparent";
    public const string LogMessage = "trace endpoint hit";
}
