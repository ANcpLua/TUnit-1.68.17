using Microsoft.Extensions.Diagnostics.Metrics.Testing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Testing;
using OpenTelemetry;
using OpenTelemetry.Metrics;

namespace TUnit.Patterns.Telemetry;

/// <summary>Framework-neutral signal testing (Microsoft.Extensions.Diagnostics.Testing + OpenTelemetry SDK) inside TUnit.</summary>
[NotInParallel(Telemetry.Key)]
public sealed class SignalsTests
{
    private const string OrderId = "order-3";
    private const string OrderIdProperty = "OrderId";
    private const int Placed = 3;

    private static readonly OrderService Service = new(NullLogger<OrderService>.Instance);

    [Test]
    public async Task Metric_collector_records_every_measurement_with_its_tags()
    {
        using var collector = new MetricCollector<long>(OrderService.Meter, OrderService.OrdersPlacedName);

        for (var i = 0; i < Placed; i++)
        {
            Service.Place(OrderId);
        }

        var measurements = collector.GetMeasurementSnapshot();
        await Assert.That(measurements).Count().IsEqualTo(Placed);
        await Assert.That(measurements.Select(static measurement => measurement.Value)).All().Satisfy(static value => value.IsEqualTo(1));
        await Assert.That(collector.LastMeasurement!.Tags[OrderService.OrderIdTag]).IsEqualTo(OrderId);
    }

    [Test]
    public async Task In_memory_metric_exporter_aggregates_the_counter()
    {
        var exported = new List<Metric>();
        using var provider = Sdk.CreateMeterProviderBuilder()
            .AddMeter(OrderService.SourceName)
            .AddInMemoryExporter(exported)
            .Build();

        for (var i = 0; i < Placed; i++)
        {
            Service.Place(OrderId);
        }

        provider.ForceFlush();

        var metric = exported.Single(static metric => metric.Name == OrderService.OrdersPlacedName);
        long sum = 0;
        foreach (ref readonly var point in metric.GetMetricPoints())
        {
            sum += point.GetSumLong();
        }

        await Assert.That(sum).IsEqualTo(Placed);
    }

    [Test]
    public async Task Fake_logger_records_structured_state()
    {
        var logger = new FakeLogger<OrderService>();

        new OrderService(logger).Place(OrderId);

        var record = logger.Collector.GetSnapshot().Single();
        await Assert.That(record.Level).IsEqualTo(LogLevel.Information);
        await Assert.That(record.Message).Contains(OrderId);
        await Assert.That(record.StructuredState!.Single(static property => property.Key == OrderIdProperty).Value).IsEqualTo(OrderId);
    }
}
