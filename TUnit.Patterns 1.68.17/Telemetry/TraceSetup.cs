using System.Collections;
using System.Diagnostics;
using OpenTelemetry.Trace;
using TUnit.Core.Logging;
using TUnit.OpenTelemetry;

namespace TUnit.Patterns.Telemetry;

/// <summary>Thread-safe ICollection for the in-memory exporter (tests export concurrently).</summary>
public sealed class ExportedSpans : ICollection<Activity>
{
    private readonly List<Activity> _spans = [];

    public int Count
    {
        get
        {
            lock (_spans)
            {
                return _spans.Count;
            }
        }
    }

    public bool IsReadOnly => false;

    public IReadOnlyList<Activity> Snapshot()
    {
        lock (_spans)
        {
            return [.. _spans];
        }
    }

    public void Add(Activity item)
    {
        lock (_spans)
        {
            _spans.Add(item);
        }
    }

    public void Clear()
    {
        lock (_spans)
        {
            _spans.Clear();
        }
    }

    public bool Contains(Activity item) => Snapshot().Contains(item);

    public void CopyTo(Activity[] array, int arrayIndex) => Snapshot().ToList().CopyTo(array, arrayIndex);

    public bool Remove(Activity item)
    {
        lock (_spans)
        {
            return _spans.Remove(item);
        }
    }

    public IEnumerator<Activity> GetEnumerator() => Snapshot().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

/// <summary>Custom ILogSink: sees every log line together with the context (test, class hook, ...) that produced it.</summary>
public sealed class RecordingSink : ILogSink
{
    public static RecordingSink Instance { get; } = new();

    private readonly List<(LogLevel Level, string Message, string? TestId)> _entries = [];

    public IReadOnlyList<(LogLevel Level, string Message, string? TestId)> Entries
    {
        get
        {
            lock (_entries)
            {
                return [.. _entries];
            }
        }
    }

    public bool IsEnabled(LogLevel level) => level >= LogLevel.Information;

    public void Log(LogLevel level, string message, Exception? exception, TUnit.Core.Context? context)
    {
        lock (_entries)
        {
            _entries.Add((level, message, (context as TestContext)?.Id));
        }
    }

    public ValueTask LogAsync(LogLevel level, string message, Exception? exception, TUnit.Core.Context? context)
    {
        Log(level, message, exception, context);
        return default;
    }
}

/// <summary>
/// TUnit.OpenTelemetry auto-wires a TracerProvider at test discovery; Configure adds our exporter and the SUT source.
/// The package pre-registers TUnitTestCorrelationProcessor, so every exported span carries tunit.test.id.
/// </summary>
public static class TraceSetup
{
    public const string AutoStartVariable = "TUNIT_OTEL_AUTOSTART";
    private const string ForceOn = "1";

    public static ExportedSpans Exported { get; } = new();

    // AutoStart runs at Order = int.MaxValue of the same hook and steps aside when any listener is already attached
    // to the "TUnit" source - which TUnit's own HTML reporter always is. TUNIT_OTEL_AUTOSTART=1 forces the provider.
    [Before(TestDiscovery, Order = int.MinValue)]
    public static void Configure()
    {
        Environment.SetEnvironmentVariable(AutoStartVariable, ForceOn);

        TUnitOpenTelemetry.Configure(builder => builder
            .AddSource(OrderService.SourceName)
            .AddInMemoryExporter(Exported));

        TUnitLoggerFactory.AddSink(RecordingSink.Instance);
    }
}
