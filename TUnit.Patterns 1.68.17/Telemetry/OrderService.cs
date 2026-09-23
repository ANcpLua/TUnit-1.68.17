using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.Logging;

namespace TUnit.Patterns.Telemetry;

/// <summary>System under test that emits all three signals: a span, a counter measurement, and a structured log.</summary>
public sealed partial class OrderService(ILogger<OrderService> logger)
{
    public const string SourceName = "tunit.patterns";
    public const string SpanName = "place-order";
    public const string OrdersPlacedName = "orders.placed";
    public const string OrderIdTag = "order.id";

    public static readonly ActivitySource Source = new(SourceName);
    public static readonly Meter Meter = new(SourceName);

    private static readonly Counter<long> OrdersPlaced = Meter.CreateCounter<long>(OrdersPlacedName);

    public void Place(string orderId)
    {
        using var span = Source.StartActivity(SpanName);
        span?.SetTag(OrderIdTag, orderId);

        OrdersPlaced.Add(1, new KeyValuePair<string, object?>(OrderIdTag, orderId));
        LogPlaced(logger, orderId);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Order {OrderId} placed")]
    private static partial void LogPlaced(ILogger logger, string orderId);
}
