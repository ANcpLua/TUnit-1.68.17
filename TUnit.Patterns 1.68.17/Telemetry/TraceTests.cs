using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using TUnit.Core.Logging;
using TUnit.Logging.Microsoft;

namespace TUnit.Patterns.Telemetry;

public static class Telemetry
{
    public const string Key = "Telemetry";
    public const string TUnitSource = "TUnit";
    public const string TestIdTag = "tunit.test.id";
    public const string TestCaseSpan = "test case";
    public const string ResultStatusTag = "test.case.result.status";
    public const string Pass = "pass";
}

[NotInParallel(Telemetry.Key)]
public sealed class TraceTests
{
    private const string OrderId = "order-1";

    private static readonly OrderService Service = new(NullLogger<OrderService>.Instance);

    [Test]
    public async Task Test_body_runs_under_the_tunit_span()
    {
        var current = Activity.Current;
        var context = TestContext.Current!;

        await Assert.That(current).IsNotNull();
        await Assert.That(current!.Source.Name).IsEqualTo(Telemetry.TUnitSource);
        await Assert.That(current.GetBaggageItem(Telemetry.TestIdTag)).IsEqualTo(context.Id);
        await Assert.That(context.Activity!.TraceId).IsEqualTo(current.TraceId);
    }

    [Test]
    public async Task Sut_spans_nest_under_the_test_and_carry_the_test_id()
    {
        var context = TestContext.Current!;
        var parent = Activity.Current!;

        Service.Place(OrderId);

        var span = TraceSetup.Exported.Snapshot().Single(activity => activity.OperationName == OrderService.SpanName && activity.TraceId == parent.TraceId);
        await Assert.That(span.ParentSpanId).IsEqualTo(parent.SpanId);
        await Assert.That(span.GetTagItem(OrderService.OrderIdTag)).IsEqualTo(OrderId);
        await Assert.That(span.GetTagItem(Telemetry.TestIdTag)).IsEqualTo(context.Id);
    }

    [Test]
    [DependsOn(nameof(Test_body_runs_under_the_tunit_span))]
    public async Task Dependency_exported_a_passed_test_case_span()
    {
        var dependency = TestContext.Current!.Dependencies.GetTests(nameof(Test_body_runs_under_the_tunit_span)).Single();

        var testCase = TraceSetup.Exported.Snapshot().Single(activity =>
            activity.OperationName == Telemetry.TestCaseSpan && Equals(activity.GetTagItem(Telemetry.TestIdTag), dependency.Id));

        await Assert.That(testCase.GetTagItem(Telemetry.ResultStatusTag)).IsEqualTo(Telemetry.Pass);
        await Assert.That(testCase.Status).IsEqualTo(ActivityStatusCode.Unset);
    }

    // Links an out-of-process trace to this test so the HTML report renders it under the test.
    [Test]
    public async Task External_traces_can_be_linked()
    {
        using var external = new Activity(OrderService.SpanName);
        external.SetParentId(ActivityTraceId.CreateRandom(), ActivitySpanId.CreateRandom());
        external.Start();

        TestContext.Current!.RegisterTrace(external.TraceId);

        await Assert.That(external.TraceId).IsNotEqualTo(TestContext.Current.Activity!.TraceId);
    }
}

[NotInParallel(Telemetry.Key)]
public sealed class LoggingTests
{
    private const string OrderId = "order-2";
    private const string Marker = "logged from a foreign async context";
    private const string DefaultLoggerMessage = "hello from the default logger";

    // TUnit.Logging.Microsoft: ILogger output lands in this test's captured output.
    [Test]
    public async Task Microsoft_logging_is_bridged_into_the_test_output()
    {
        using var factory = Microsoft.Extensions.Logging.LoggerFactory.Create(builder => builder.AddTUnit(TestContext.Current!));

        new OrderService(factory.CreateLogger<OrderService>()).Place(OrderId);

        await Assert.That(TestContext.Current!.GetStandardOutput()).Contains(OrderId);
    }

    [Test]
    public async Task Default_logger_reaches_output_and_custom_sinks()
    {
        TestContext.Current!.GetDefaultLogger().LogInformation(DefaultLoggerMessage);

        await Assert.That(TestContext.Current.GetStandardOutput()).Contains(DefaultLoggerMessage);
        await Assert.That(RecordingSink.Instance.Entries)
            .Contains(entry => entry.Level == TUnit.Core.Logging.LogLevel.Information && entry.Message.Contains(DefaultLoggerMessage) && entry.TestId == TestContext.Current.Id);
    }

    // Work on a thread that did not inherit the test's AsyncLocal re-attaches with GetById + MakeCurrent.
    [Test]
    public async Task Make_current_reattaches_output_from_a_foreign_context()
    {
        var id = TestContext.Current!.Id;
        Task work;

        using (ExecutionContext.SuppressFlow())
        {
            work = Task.Run(() =>
            {
                using (TestContext.GetById(id)!.MakeCurrent())
                {
                    Console.WriteLine(Marker);
                }
            });
        }

        await work;

        await Assert.That(TestContext.Current.GetStandardOutput()).Contains(Marker);
    }
}
