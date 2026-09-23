# TUnit.Patterns — the extensibility surface, end to end

Runnable, self-verifying examples of the patterns behind TUnit's
[docs](https://tunit.dev/docs/intro), pinned to TUnit 1.68.17. Every test asserts the behaviour
it demonstrates (display names, execution order, retry history, receiver timing), and every string
that matters is a `const`.

| Folder | Pattern | API |
|---|---|---|
| `Ordering/` | Test dependencies with state hand-over (same class and cross-class); `ProceedOnFailure` (explicit-only showcase); serial ordered steps | `[DependsOn]`, `[DependsOn<T>]`, `ProceedOnFailure`, `TestContext.Dependencies.GetTests`, `StateBag`, `[NotInParallel(key, Order = n)]` |
| `Parallelism/` | Concurrency cap shared by every test naming the limiter; mutual exclusion by key; phased groups that never overlap each other | `[ParallelLimiter<T>]` + `IParallelLimit`, `[NotInParallel(key)]`, `[ParallelGroup]`, `[Repeat]` |
| `Retry/` | Custom retry policy via `ShouldRetry`; built-in type filter and backoff; per-attempt history | `RetryAttribute`, `RetryOnExceptionTypes`, `BackoffMs`, `Execution.CurrentRetryAttempt`, `Execution.RetryAttempts` |
| `Data/` | Row metadata (display name, skip, category); `$parameter` templates + argument formatters; Cartesian mixing of sources; deferred enumeration; async typed generator; untyped generator supplying a label and array; generic method source; lowest-level typed source; keyed fixtures that know their key | `TestDataRow<T>`, `[DisplayName]`, `[ArgumentDisplayFormatter<T>]`, `[CombinedDataSources]`, `DeferEnumeration`, `AsyncDataSourceGeneratorAttribute<T1,T2>`, `UntypedDataSourceGeneratorAttribute`, `[MethodDataSource<T>]`, `TypedDataSourceAttribute<T>`, `IKeyedDataSource` |
| `Extensions/` | Ambient scope around the test body; event receivers on attributes with stage control; registration-time conditional skip; hook executors; registration/discovery/last-test receivers; executors that register themselves and explicit limiters that beat them; display-name formatter; run-time skips; command-line parameters; timeout + retry interplay | `ITestExecutor` + `[TestExecutor<T>]`, `[Culture]`, `ITestStartEventReceiver` / `ITestEndEventReceiver` + `EventReceiverStage.Early`, `SkipAttribute.ShouldSkip`, `IHookExecutor` + `[HookExecutor<T>]`, `ITestRegisteredEventReceiver` (`SetHookExecutor`, `SetTestExecutor`, `SetParallelLimiter`), `ITestDiscoveryEventReceiver`, `ILastTestIn*EventReceiver`, `DisplayNameFormatterAttribute`, `Skip.Test/When/Unless`, `TestContext.Parameters`, `[Timeout]` + `[Retry]` |
| `Assertions/` | Source-generated assertions from predicates and from existing methods, chained with `And` / `Or`; assignability of `Type` values; multiple invalid fields reported together with `Assert.Multiple` | `[GenerateAssertion]`, `[AssertionFrom<T>]`, `AssertionResult`, `Assert.That(Type).IsAssignableTo<T>()` / `IsAssignableFrom<T>()` / `IsAssignableTo(Type)`, `Assert.Multiple()` |
| `Context/` | Custom properties, isolation helpers, test- and session-level artifacts in the platform results directory, discovery-to-execution state | `[Property]`, `Isolation.GetIsolatedName` / `GetIsolatedPrefix`, `TestContext.ResultsDirectory`, `Output.AttachArtifact`, `TestSessionContext.AddArtifact`, `TestBuilderContext.Current.StateBag` |
| `Dynamic/` | Tests assembled in code | `[DynamicTestBuilder]`, `DynamicTest<T>`, `DynamicTestHelper.Argument<T>()` |
| `Fixtures/` | Nested property injection with dependency-ordered async init; keyed sharing; discovery-time initialization feeding an instance data source | `[ClassDataSource<T>]` on properties, `SharedType.Keyed`, `IAsyncInitializer`, `IAsyncDiscoveryInitializer`, `[InstanceMethodDataSource]` |
| `Telemetry/` | TUnit's own spans (`TUnit` source, `tunit.test.id` baggage, exported `test case` span), SUT spans nested and correlated, external trace links; `ILogger` bridged into test output, custom `ILogSink`, foreign-context re-attachment; framework-neutral metric/log testing | `TUnit.OpenTelemetry` (`TUnitOpenTelemetry.Configure`, `TUnitTestCorrelationProcessor`), `TestContext.Activity`, `RegisterTrace`, `TUnit.Logging.Microsoft` (`AddTUnit`), `GetDefaultLogger`, `TUnitLoggerFactory.AddSink`, `GetStandardOutput`, `GetById` + `MakeCurrent`, `MetricCollector<T>`, `FakeLogger<T>`, OpenTelemetry in-memory exporters |

Usage notes for the pinned version:

- `[AssertionFrom]` copies `{parameter}` placeholders verbatim into the generated expectation
  message — only `[GenerateAssertion]` substitutes them — so those messages are kept literal.
- `ITestRetryEventReceiver` exists in `TUnit.Core` but nothing in the engine raises it; retry
  history is read from `TestContext.Execution.RetryAttempts` instead.
- `TestDataRowTests` deliberately skips one row (`Skip:` metadata) and `Skip_test_skips_at_run_time` skips itself, so a full run reports two skips.
- `ProceedOnFailureShowcase` and `Inconclusive_at_run_time` are `[Explicit]` because they need a failing / inconclusive result; run them with a `--treenode-filter`. `InconclusiveTestException` is reported as *failed* on 1.68.17.
- `TUnit.OpenTelemetry`'s auto-start steps aside whenever a listener is already attached to the `TUnit` source, which the built-in HTML reporter always is; `TraceSetup` sets `TUNIT_OTEL_AUTOSTART=1` before the provider is built.
- An assembly-level `[ParallelLimiter<T>]` replaces class- and method-level ones on 1.68.17 (docs: Method > Class > Assembly); the assembly-level form therefore lives in `TUnit.Patterns.Policies 1.68.17` together with the other assembly-wide attributes.
- `Assert.That(typeof(X))` evaluates the represented type only for the first assignability check; behind `.And` / `.Or` it inspects `RuntimeType` again (`TypeAssertionTests.Chained_assignability_inspects_the_runtime_type`).

Run it with:

```bash
dotnet run --project "TUnit.Patterns 1.68.17/TUnit.Patterns 1.68.17.csproj"
```
