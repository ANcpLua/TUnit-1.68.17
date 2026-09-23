# TUnit 1.68.17 API examples

Each table maps a current API to a runnable example and the assertion that demonstrates it.
All TUnit packages are pinned to 1.68.17. See [README.md](README.md) for projects, commands,
and the latest verification counts.

Paths are relative to the project named in the section unless a full repository path is given.
Intentional failure and cancellation demonstrations are marked `[Explicit]` and run separately.

### Ordering and dependencies — `TUnit.Patterns 1.68.17/Ordering`

| API | Namespace | Kind | Example | Verified by | Note |
|---|---|---|---|---|---|
| `DependsOnAttribute(string)` (repeatable) | `TUnit.Core` | attribute | `Ordering/DependsOnTests.cs` | `DependsOnTests.ShipOrder` |  |
| `TestContext.Dependencies.GetTests(string)` | `TUnit.Core` | method | `Ordering/DependsOnTests.cs` | `DependsOnTests.PayOrder` | returns all invocations of the dependency |
| `TestContext.StateBag` | `TUnit.Core` | property | `Ordering/DependsOnTests.cs` | `DependsOnTests.PayOrder` | hand-over between dependent tests |
| `TestContext.Execution.Result.State` / `TestState` | `TUnit.Core` | property | `Ordering/DependsOnTests.cs` | `DependsOnTests.ShipOrder` |  |
| `NotInParallelAttribute(string key) { Order }` | `TUnit.Core` | attribute | `Ordering/DependsOnTests.cs` | `NotInParallelOrderTests` | serial, ordered, only among the key |
| `DependsOnAttribute<TClass>(string)` | `TUnit.Core` | attribute | `Ordering/DependsOnTests.cs` | `CrossClassDependsOnTests` | cross-class dependency |
| `DependsOnAttribute.ProceedOnFailure` + `Dependencies.GetTests(...).Execution.Result.Exception` | `TUnit.Core` | property | `Ordering/DependsOnTests.cs` | `ProceedOnFailureShowcase` (`[Explicit]`, run via tree-node filter: 1 failed, 1 passed) |  |

### Parallelism — `TUnit.Patterns 1.68.17/Parallelism`

| API | Namespace | Kind | Example | Verified by | Note |
|---|---|---|---|---|---|
| `ParallelLimiterAttribute<T>` + `IParallelLimit.Limit` | `TUnit.Core` / `TUnit.Core.Interfaces` | attribute, interface | `Parallelism/ParallelismTests.cs` | `ParallelLimiterTests` (peak ≤ 2 over 10 runs) | limit shared by every test naming the type |
| `RepeatAttribute(int)` | `TUnit.Core` | attribute | `Parallelism/ParallelismTests.cs` | `ParallelLimiterTests` | `(RepeatIndex: n)` display suffix |
| `NotInParallelAttribute(string key)` | `TUnit.Core` | attribute | `Parallelism/ParallelismTests.cs` | `NotInParallelKeyTests` (holders == 1) |  |
| `ParallelGroupAttribute(string)` | `TUnit.Core` | attribute | `Parallelism/ParallelGroupTests.cs` | `UserRepositoryTests`, `OrderRepositoryTests`, `PaymentApiTests` (other group count == 0) | groups never overlap each other |
| `[assembly: ParallelLimiter<T>]` | `TUnit.Core` | attribute | `TUnit.Patterns.Policies 1.68.17/AssemblyPolicies.cs` | `PolicyTests` | replaces class- and method-level limiters on 1.68.17 (see divergences) |
| `[assembly: NotInParallel]` | `TUnit.Core` | attribute | `TUnit.Patterns.Policies 1.68.17/AssemblyPolicies.cs` | `PolicyTests`, `SecondClassTests` (running == 1 across classes) |  |

### Retry — `TUnit.Patterns 1.68.17/Retry`

| API | Namespace | Kind | Example | Verified by | Note |
|---|---|---|---|---|---|
| `RetryAttribute.ShouldRetry(TestContext, Exception, int)` | `TUnit.Core` | method | `Retry/RetryOnTransientAttribute.cs` | `RetryTests.Custom_policy_retries_transient_failures_until_they_pass` |  |
| `RetryAttribute.RetryOnExceptionTypes` | `TUnit.Core` | property | `Retry/RetryTests.cs` | `RetryTests.Built_in_policy_filters_by_exception_type` | `IsAssignableFrom` match |
| `RetryAttribute.BackoffMs` / `BackoffMultiplier` | `TUnit.Core` | property | `Retry/RetryTests.cs` | same | exponential backoff |
| `TestContext.Execution.CurrentRetryAttempt` | `TUnit.Core.Interfaces.ITestExecution` | property | `Retry/RetryTests.cs` | `RetryTests` | 0-based |
| `TestContext.Execution.RetryAttempts` | `TUnit.Core.Interfaces.ITestExecution` | property | `Retry/RetryTests.cs` | `RetryTests` | failed prior attempts, empty when none |
| `[Timeout]` + `[Retry]` interplay (fresh timeout per attempt) | `TUnit.Core` | attribute | `Extensions/RegistrationTests.cs` | `TimeoutRetryTests` (attempt 0 times out, attempt 1 passes) |  |
| `[assembly: Retry(n)]` → `Metadata.TestDetails.RetryLimit` | `TUnit.Core` | attribute, property | `TUnit.Patterns.Policies 1.68.17/AssemblyPolicies.cs` | `PolicyTests.Assembly_wide_retry_limit_is_visible_on_the_test_details` |  |
| `[assembly: Timeout(ms)]` → `Metadata.TestDetails.Timeout` | `TUnit.Core` | attribute, property | `TUnit.Patterns.Policies 1.68.17/AssemblyPolicies.cs` | `PolicyTests.Assembly_wide_timeout_is_visible_on_the_test_details` |  |

### Data sources — `TUnit.Patterns 1.68.17/Data`, `TUnit.Mixed 1.68.17`

| API | Namespace | Kind | Example | Verified by | Note |
|---|---|---|---|---|---|
| `TestDataRow<T>(value, DisplayName:, Skip:, Categories:)` | `TUnit.Core` | class | `Data/DataTests.cs` | `TestDataRowTests` (`Admin login`, one skip, `[Category=Security]` filter) | tuple `T` is spread across parameters |
| `TestContext.Metadata.DisplayName` | `TUnit.Core` | property | `Data/DataTests.cs`, `Context/ContextTests.cs` | `TestDataRowTests`, `DisplayNameTests` | docs' `GetDisplayName()` does not exist |
| `DisplayNameAttribute("$param …")` | `TUnit.Core` | attribute | `Data/DataTests.cs` | `DisplayNameTests` (`1+2 equals 3`) |  |
| `ArgumentDisplayFormatter.CanHandle/FormatValue` + `ArgumentDisplayFormatterAttribute<T>` | `TUnit.Core` | class, attribute | `Data/SumFormatter.cs` | `DisplayNameTests` | composes with `$param` |
| `CombinedDataSourcesAttribute` + per-parameter `[Arguments]` / `[MethodDataSource]` | `TUnit.Core` | attribute | `Data/DataTests.cs` | `CombinedDataSourceTests` (8 cases) | Cartesian product |
| `MethodDataSourceAttribute.DeferEnumeration` | `TUnit.Core` | property | `Data/DataTests.cs` | `DeferredEnumerationTests` (`--list-tests` = 1 node) | rows nest under a placeholder |
| `AsyncDataSourceGeneratorAttribute<T1, T2>.GenerateDataSourcesAsync` | `TUnit.Core` | class | `Data/FibonacciAttribute.cs` | `AsyncGeneratorTests` | runs at discovery |
| `UntypedDataSourceGeneratorAttribute.GenerateDataSources` | `TUnit.Core` | class | `Data/BatchesAttribute.cs` | `UntypedGeneratorTests.Untyped_source_supplies_a_label_and_array` |  |
| `ArgumentsAttribute` (method and class level) | `TUnit.Core` | attribute | `TUnit.Mixed 1.68.17/DataDrivenTests.cs` | `DataDrivenTests`, `ClassLevelArgumentTests` |  |
| `MethodDataSourceAttribute` (tuple rows) | `TUnit.Core` | attribute | `TUnit.Mixed 1.68.17/DataDrivenTests.cs` | `DataDrivenTests.Subtract_WithMethodDataSource` |  |
| `DataSourceGeneratorAttribute<T1, T2, T3>` | `TUnit.Core` | class | `TUnit.Mixed 1.68.17/Data/AdditionDataGenerator.cs` | `DataDrivenTests.Add_WithCustomDataGenerator` |  |
| `MatrixDataSourceAttribute` + `MatrixAttribute` | `TUnit.Core` | attribute | `TUnit.Mixed 1.68.17/DataDrivenTests.cs` | `DataDrivenTests.Multiply_AllCombinations` |  |
| `MethodDataSourceAttribute<TClass>(string)` | `TUnit.Core` | attribute | `Data/MoreDataSources.cs` | `MoreDataSourceTests.Generic_method_data_source` | source class must be non-static |
| `TypedDataSourceAttribute<T>.GetTypedDataRowsAsync` | `TUnit.Core` | class | `Data/MoreDataSources.cs` | `MoreDataSourceTests.Typed_data_source_yields_instances` | `IAsyncEnumerable<Func<Task<T>>>` |
| `IKeyedDataSource.Key` (set before `InitializeAsync`) | `TUnit.Core.Interfaces` | interface | `Data/MoreDataSources.cs` | `MoreDataSourceTests.Keyed_fixture_knows_its_key` | non-nullable `string` |
| `TestBuilderContext.Current.StateBag` / `TestMetadata.Name` | `TUnit.Core` | property | `Context/SessionArtifacts.cs` | `TestBuilderContextTests` | discovery-time state copied into `TestContext.StateBag` |

### Extension points — `TUnit.Patterns 1.68.17/Extensions`, `TUnit.Mixed 1.68.17/CancellationTests.cs`

| API | Namespace | Kind | Example | Verified by | Note |
|---|---|---|---|---|---|
| `ITestExecutor.ExecuteTest(TestContext, Func<ValueTask>)` + `TestExecutorAttribute<T>` | `TUnit.Core.Interfaces` / `TUnit.Core` | interface, attribute | `Extensions/ScopedCultureExecutor.cs` | `TestExecutorTests.Executor_establishes_the_ambient_culture` |  |
| `CultureAttribute(string)` | `TUnit.Core.Executors` | attribute | `Extensions/ExtensionTests.cs` | `TestExecutorTests.Built_in_culture_attribute_does_the_same` |  |
| `ITestStartEventReceiver` / `ITestEndEventReceiver` on an attribute | `TUnit.Core.Interfaces` | interface | `Extensions/StopwatchAttribute.cs` | `EventReceiverTests` | `Order` required |
| `EventReceiverStage.Early` | `TUnit.Core.Enums` | enum | `Extensions/StopwatchAttribute.cs` | `EventReceiverTests` (start before `[Before(Test)]`, end before `[After(Test)]`) |  |
| `SkipAttribute.ShouldSkip(TestRegisteredContext)` | `TUnit.Core` | method | `Extensions/SkipOnAttribute.cs` | `SkipTests` | registration-time decision |
| `[Before(Test)]` / `[After(Test)]` with `TestContext` parameter | `TUnit.Core` | attribute | `Extensions/ExtensionTests.cs`, `TUnit.Mixed 1.68.17/BasicTests.cs` | `EventReceiverTests` |  |
| `[Before(Class)]` / `[After(Class)]` (`ClassHookContext`), `[Before(TestSession)]` / `[After(TestSession)]` (`TestSessionContext`) | `TUnit.Core` | attribute | `TUnit.Mixed 1.68.17/BasicTests.cs`, `HooksAndLifecycle.cs` | run of `TUnit.Mixed` |  |
| `TestContext.Execution.Cancel()` | `TUnit.Core` | method | `TUnit.Mixed 1.68.17/CancellationTests.cs` | `PerTestCancellationShowcase` (`[Explicit]`) | per-test cancellation |
| `TestContext.Execution.AddLinkedCancellationToken` | `TUnit.Core` | method | `TUnit.Mixed 1.68.17/CancellationTests.cs` | `BeforeHookLinkedCancellationShowcase`, `ExecutorLinkedCancellationShowcase` | honoured from hooks and executors |
| `TimeoutAttribute(int)` + injected `CancellationToken` | `TUnit.Core` | attribute | `TUnit.Mixed 1.68.17/CancellationTests.cs` | same |  |
| exception thrown while a test handles its timeout kept in the result | engine | — | `TUnit.Mixed 1.68.17/CancellationTests.cs` | `TimeoutDiagnosticsShowcase` (`[Explicit]`) | adds diagnostic context to cancellation |
| `ExplicitAttribute` | `TUnit.Core` | attribute | `TUnit.Mixed 1.68.17/CancellationTests.cs` | opt-in run |  |
| `IHookExecutor` (10 methods, `MethodMetadata.Name`) + `HookExecutorAttribute<T>` | `TUnit.Core.Interfaces` / `TUnit.Core.Executors` | interface, attribute | `Extensions/RecordingHookExecutor.cs`, `Extensions/RegistrationTests.cs` | `RegistrationTests.Hook_ran_through_the_custom_executor` |  |
| `ITestRegisteredEventReceiver` + `TestRegisteredContext.SetHookExecutor` / `SetParallelLimiter` | `TUnit.Core.Interfaces` / `TUnit.Core` | interface, method | `Extensions/RegistrationAttributes.cs` | `RegistrationTests.Registration_receiver_installed_the_hook_executor` (`Execution.CustomHookExecutor`) |  |
| `TestRegisteredContext.SetTestExecutor` with an executor that is also an `ITestRegisteredEventReceiver` | `TUnit.Core` | method | `Extensions/ExecutorRegistration.cs` | `ExecutorRegistrationTests.Registered_executor_supplies_the_default_limiter` | registration configures the executor and its default limiter |
| explicit `[ParallelLimiter<T>]` over `SetParallelLimiter` from a receiver or executor | `TUnit.Core` | attribute | `Extensions/ExecutorRegistrationTests.cs` | `ExecutorRegistrationTests.Explicit_limiter_beats_the_executor_default` | independent of callback order; an explicit limit may also be wider |
| `ITestDiscoveryEventReceiver.OnTestDiscovered` + `DiscoveredTestContext.TestContext.StateBag` | `TUnit.Core.Interfaces` | interface | `Extensions/RegistrationAttributes.cs` | `RegistrationTests.Discovery_receiver_assigned_an_id` | docs' Global Test IDs pattern |
| `ILastTestInClassEventReceiver` / `ILastTestInAssemblyEventReceiver` / `ILastTestInTestSessionEventReceiver` | `TUnit.Core.Interfaces` | interface | `Extensions/RegistrationAttributes.cs` | `RegistrationTests.Last_test_in_class_was_observed` (`[After(Class)]`) |  |
| `DisplayNameFormatterAttribute.FormatDisplayName(DiscoveredTestContext)` | `TUnit.Core` | class | `Extensions/RegistrationAttributes.cs` | `RegistrationTests.Formatter_rewrites_the_display_name` (`--list-tests` shows `[…]`) |  |
| `Skip.When` / `Skip.Unless` / `Skip.Test` | `TUnit.Core` | method | `Extensions/RegistrationTests.cs` | `SkipAndInconclusiveTests` (one run-time skip) |  |
| `InconclusiveTestException` | `TUnit.Core.Exceptions` | class | `Extensions/RegistrationTests.cs` | `SkipAndInconclusiveTests.Inconclusive_at_run_time` (`[Explicit]`) | reported as failed on 1.68.17 |
| `TestContext.Parameters.TryGetValue` (`--test-parameter key=value`) | `TUnit.Core` | property | `Extensions/RegistrationTests.cs` | `SkipAndInconclusiveTests.Test_parameters_come_from_the_command_line` (run with `environment=staging`) |  |
| `[assembly: Culture]` / `[assembly: Category]` | `TUnit.Core.Executors` / `TUnit.Core` | attribute | `TUnit.Patterns.Policies 1.68.17/AssemblyPolicies.cs` | `PolicyTests` | `Metadata.TestDetails.Categories` |

### Assertions — `TUnit.Patterns 1.68.17/Assertions`

| API | Namespace | Kind | Example | Verified by | Note |
|---|---|---|---|---|---|
| `GenerateAssertionAttribute(ExpectationMessage = "… {param}")` on `bool` / `AssertionResult` methods | `TUnit.Assertions.Attributes` | attribute | `Assertions/OrderAssertions.cs` | `AssertionTests` | placeholders substituted |
| `AssertionResult.Passed` / `Failed(string)` | `TUnit.Assertions.Core` | class | `Assertions/OrderAssertions.cs` | `AssertionTests.Generated_assertion_reports_its_custom_failure` |  |
| `AssertionFromAttribute<T>(Type, string, CustomName, NegateLogic, ExpectationMessage)` | `TUnit.Assertions.Attributes` | attribute | `Assertions/OrderAssertions.cs` | `AssertionTests.Lifted_assertions_and_their_negation` | `{param}` placeholders NOT substituted — keep literal |
| `.And` / `.Or` chaining on generated assertions | `TUnit.Assertions` | — | `Assertions/AssertionTests.cs` | `AssertionTests.Generated_assertions_chain`, `Or_short_circuits_on_the_first_pass` |  |
| `Assert.That(Func<Task>).Throws<T>()` → `AssertionException` | `TUnit.Assertions.Exceptions` | method | `Assertions/AssertionTests.cs` | same |  |
| `.All().Satisfy(x => x.IsEqualTo(...))` | `TUnit.Assertions` | method | `Retry/RetryTests.cs` | `RetryTests` | lambda receives an `IAssertionSource<T>` |
| `Assert.That(Type?)` → `TypeValueAssertion.IsAssignableTo<T>` / `IsAssignableFrom<T>` / `IsNotAssignableTo<T>` / `IsNotAssignableFrom<T>` | `TUnit.Assertions.Sources` | class, method | `Assertions/TypeAssertionTests.cs` | `TypeAssertionTests.Generic_assignability_evaluates_the_represented_type` | evaluates the represented type |
| `IsAssignableTo(Type)` / `IsAssignableFrom(Type)` on a `Type` source | `TUnit.Assertions.Extensions` | method | `Assertions/TypeAssertionTests.cs` | `TypeAssertionTests.Runtime_type_overloads_take_a_type_argument`, `Failure_names_the_represented_types` | source-generated from `TypeAssertionExtensions` |
| type assignability behind `.And` / `.Or` | `TUnit.Assertions` | — | `Assertions/TypeAssertionTests.cs` | `TypeAssertionTests.Chained_assignability_inspects_the_runtime_type` | checks `RuntimeType` again (see divergences) |
| `Assert.Multiple()` | `TUnit.Assertions` | method | `Assertions/MultipleTests.cs` | `MultipleTests.Multiple_assertions_report_all_invalid_order_fields` | reports both invalid fields together |

### Context and artifacts — `TUnit.Patterns 1.68.17/Context`

| API | Namespace | Kind | Example | Verified by | Note |
|---|---|---|---|---|---|
| `PropertyAttribute(name, value)` + `Metadata.TestDetails.CustomProperties` | `TUnit.Core` | attribute, property | `Context/ContextTests.cs` | `ContextTests.Custom_properties_are_readable_at_run_time`, `--treenode-filter "/*/*/*/*[Owner=platform]"` |  |
| `TestContext.Isolation.UniqueId` / `GetIsolatedName` / `GetIsolatedPrefix` | `TUnit.Core.Interfaces.ITestIsolation` | property, method | `Context/ContextTests.cs` | `ContextTests.Isolation_helpers_produce_unique_resource_names` | `test_{id}_{name}` · `test{sep}{id}{sep}` |
| `TestContext.ResultsDirectory` (static) | `TUnit.Core` | property | `Context/ContextTests.cs` | `ContextTests.Artifacts_land_in_the_results_directory` | honours `--results-directory` |
| `TestContext.Output.AttachArtifact(path, displayName:, description:)` | `TUnit.Core.Interfaces.ITestOutput` | method | `Context/ContextTests.cs` | same (artifact listed in run output) |  |
| `TestSessionContext.Current.AddArtifact(Artifact)` | `TUnit.Core` | method | `Context/SessionArtifacts.cs` | run output lists `session-info.txt` | `[Before(TestSession)]` |

### Dynamic tests — `TUnit.Patterns 1.68.17/Dynamic`

| API | Namespace | Kind | Example | Verified by | Note |
|---|---|---|---|---|---|
| `DynamicTestBuilderAttribute` + `DynamicTestBuilderContext.AddTest` | `TUnit.Core` | attribute, method | `Dynamic/DynamicTests.cs` | `DynamicTests.Greets` ×3 |  |
| `DynamicTest<T> { TestMethod, TestMethodArguments, Attributes }` + `DynamicTestHelper.Argument<T>()` | `TUnit.Core` | class | `Dynamic/DynamicTests.cs` | same | lambda is an expression, not a delegate |

### Fixtures and injection — `TUnit.Patterns 1.68.17/Fixtures`, `TUnit.Mixed 1.68.17`, `TUnit 1.68.17`

| API | Namespace | Kind | Example | Verified by | Note |
|---|---|---|---|---|---|
| `ClassDataSourceAttribute<T>` on a `required` property (test class and fixture) | `TUnit.Core` | attribute | `Fixtures/Fixtures.cs`, `Fixtures/FixtureTests.cs` | `NestedInjectionTests.Nested_property_is_initialized_before_its_owner` | dependency-ordered init |
| `SharedType.Keyed` + `Key` | `TUnit.Core` | enum, property | `Fixtures/Fixtures.cs` | `NestedInjectionTests.Keyed_sharing_hands_out_the_same_instance` | same instance across property and parameter injection |
| `SharedType.PerClass` / `PerTestSession` | `TUnit.Core` | enum | `TUnit.Mixed 1.68.17/DependencyInjectionTests.cs`, `TUnit 1.68.17/Tests.cs` | run of those projects |  |
| `IAsyncInitializer` / `IAsyncDisposable` on fixtures | `TUnit.Core.Interfaces` | interface | `Fixtures/Fixtures.cs`, `TUnit.Mixed 1.68.17/Data/InMemoryDb.cs` | `NestedInjectionTests` |  |
| `IAsyncDiscoveryInitializer` + `InstanceMethodDataSourceAttribute` | `TUnit.Core.Interfaces` / `TUnit.Core` | interface, attribute | `Fixtures/Fixtures.cs`, `Fixtures/FixtureTests.cs` | `DiscoveryInitializerTests` (2 rows discovered) | discovery-time init |
| `WebApplicationFactory<Program>` as `ClassDataSource` + `IAsyncInitializer` | `Microsoft.AspNetCore.Mvc.Testing` | class | `TUnit 1.68.17/TUnit 1.68.17/WebApplicationFactory.cs` | `Tests.Test` |  |

### TUnit.Mocks — `TUnit.Mocks 1.68.17`

| API | Namespace | Kind | Example | Verified by | Note |
|---|---|---|---|---|---|
| `T.Mock()` static extension; wrapper *is* the interface | `TUnit.Mocks` | method | `MockEssentials.cs` | `MockEssentials.Known_user_gets_a_welcome_mail` | C# 14 static extension |
| `Returns(value)` / `Any()` / `Any<T>()` / inline lambda matchers | `TUnit.Mocks` | method | `MockEssentials.cs` | `MockEssentials.Inline_lambdas_are_argument_matchers` |  |
| captured matcher `.Values` / `.Latest` | `TUnit.Mocks` | property | `MockEssentials.cs` | `MockEssentials.Captured_arguments_are_inspectable` |  |
| `.Throws<T>().Then().Returns(...)` | `TUnit.Mocks` | method | `MockEssentials.cs` | `MockEssentials.Sequential_setups_model_flaky_dependencies` |  |
| `WasCalled(Times.Once)` / `WasNeverCalled()` | `TUnit.Mocks` | method | `MockEssentials.cs` | `MockEssentials` |  |
| `MockBehavior.Strict` → `MockStrictBehaviorException` | `TUnit.Mocks` / `TUnit.Mocks.Exceptions` | enum, class | `MockEssentials.cs` | `MockEssentials.Strict_mocks_reject_surprise_calls` |  |
| `SetState` / `InState` / `TransitionsTo` | `TUnit.Mocks` | method | `StatefulConnectionTests.cs` | `StatefulConnectionTests.Status_follows_the_connection_state_machine` |  |
| `.Raises{Event}(args)` / `Raise{Event}(args)` / `Events.{Event}.WasSubscribed` / `SubscriberCount` | `TUnit.Mocks` | method, property | `StatefulConnectionTests.cs` | `StatefulConnectionTests` | generated per event |
| `Returns(async () => …)` keeps the task pending | `TUnit.Mocks` | method | `PendingTaskTests.cs` | `PendingTaskTests.Caller_timeout_beats_a_hanging_feed` | async factories run once per call |
| runtime auto-stubs for ungenerated interfaces | `TUnit.Mocks` | — | `RuntimeAutoStubTests.cs` | `RuntimeAutoStubTests` | requires dynamic code and the SDK's `DynamicProxyGenAssembly2` internals grant |
| `<TUnitMocksExperimentalInternalsAccess>` + `<TUnitMocksInternalsAccess Include>` | MSBuild | config | `TUnit.Mocks 1.68.17.csproj`, `InternalsAccessTests.cs` | `InternalsAccessTests` | configure internal SDK types through the publicized compiler reference |
| `T.Mock()` on interfaces with `static abstract` members → `Mock<TMockable>`; pass `.Object` | `TUnit.Mocks` | method | `StaticAbstractTests.cs` | `StaticAbstractTests` | wrapper is not the interface here |
| `Raise{Event}(args)` with a `ref struct` argument (`EventHandler<TRefStruct>`, `ReadOnlySpan<T>` delegate) | `TUnit.Mocks` | method | `RefStructEventTests.cs` | `RefStructEventTests.Ref_struct_payload_reaches_the_subscriber`, `Span_argument_is_raised_without_copying_to_the_heap` | typed dispatch, no boxing |
| `Raise{Event}(ref arg)` on a delegate with `ref` / `in` / `out` parameters | `TUnit.Mocks` | method | `RefStructEventTests.cs` | `RefStructEventTests.Ref_argument_changes_reach_the_caller_and_later_subscribers` (10 − 3 − 4 = 3) | modifiers kept; changes reach the caller and later subscribers |
| `.Callback(() => mock.Raise{Event}(new …))` instead of `.Raises{Event}(args)` for stack-only arguments | `TUnit.Mocks` | method | `RefStructEventTests.cs` | `RefStructEventTests.Callback_creates_the_argument_when_the_setup_runs` | no deferred `.Raises{Event}` is generated for these events |
| `init` properties and indexers on mocked interfaces and classes (`mock.Prop.Returns`, `mock.Item(key).Returns`, `.Setter.WasNeverCalled()`) | `TUnit.Mocks` | method, property | `InitOnlyMemberTests.cs` | `InitOnlyMemberTests.Init_only_property_and_indexer_are_configurable`, `Unconfigured_virtual_init_property_keeps_the_base_value` (25 → 100) | configure getters while retaining init-only setters |
| `Mock.Wrap(instance)` | `TUnit.Mocks` | method | `WrappedInstanceTests.cs` | `WrappedInstanceTests.Wrapped_instance_forwards_calls_and_records_them` (real 100, `WasCalled(Times.Once)`) | unconfigured wrapped calls reach the real instance |

### TUnit.Playwright — `TUnit.Playwright 1.68.17` (run with browsers installed by `Hooks.cs`)

| API | Namespace | Kind | Example | Verified by | Note |
|---|---|---|---|---|---|
| `PageTest` base + `Page` / `Expect(...)` | `TUnit.Playwright` | class | `Tests.cs` | `Tests.Test` | needs network (playwright.dev) |
| `PageFixture` via `[ClassDataSource<PageFixture>]` ×2 (shared browser, isolated contexts) | `TUnit.Playwright` | class | `TwoContextFixtureTests.cs` | `TwoContextFixtureTests.Two_Pages_Have_Isolated_Storage_But_Share_Browser` |  |
| `Microsoft.Playwright.Program.Main(["install"])` in `[Before(TestSession)]` | `Microsoft.Playwright` | method | `Hooks.cs` | run of `TUnit.Playwright 1.68.17` |  |
| `RecordVideoAttribute(path, width, height)` on a `PageTest` method | `TUnit.Playwright` | attribute | `VideoRecordingTests.cs` | `VideoRecordingTests.Recorded_page_uses_the_recording_viewport` (`Page.Video` set, viewport 640×360) | method-only; unmarked tests do not record (`Tests_without_the_attribute_do_not_record`) |
| recording renamed to `{TestName}.webm` (or `{TestName}-{n}.webm`) and attached via `Output.AttachArtifact` | `TUnit.Playwright` | — | `VideoRecordingTests.cs` | `VideoRecordingTests.Recording_is_named_after_the_test_and_attached` (`[DependsOn]`; reads `Output.Artifacts` of the recorded test) | finalised after teardown; retries get `-attempt{n}`, extra pages `-{i}`; never overwrites — a rerun into the same directory adds `-2`, `-3`, … |
| `[RecordVideo]` on a per-test `PageFixture` | `TUnit.Playwright` | attribute | `VideoRecordingTests.cs` | `FixtureVideoRecordingTests.Fixture_page_is_recorded_at_the_default_size` (1280×1400) | fixture must stay `SharedType.None` |

### Telemetry — traces, logs, metrics (`TUnit.Patterns 1.68.17/Telemetry`)

| API | Namespace | Kind | Example | Verified by | Note |
|---|---|---|---|---|---|
| `TUnitOpenTelemetry.Configure(Action<TracerProviderBuilder>)` in `[Before(TestDiscovery, Order = int.MinValue)]` | `TUnit.OpenTelemetry` | method | `Telemetry/TraceSetup.cs` | `TraceTests` | auto-start runs at `Order = int.MaxValue` |
| `TUNIT_OTEL_AUTOSTART=1` | env | config | `Telemetry/TraceSetup.cs` | `TraceTests.Sut_spans_nest_under_the_test_and_carry_the_test_id` | required: the HTML reporter already listens to `TUnit`, which makes auto-start step aside |
| `Activity.Current` under the `TUnit` source with `tunit.test.id` baggage; `TestContext.Current.Activity` | `System.Diagnostics` / `TUnit.Core` | property | `Telemetry/TraceTests.cs` | `TraceTests.Test_body_runs_under_the_tunit_span` | net8.0+ |
| `TUnitTestCorrelationProcessor` (pre-registered) tags SUT spans with `tunit.test.id` | `TUnit.OpenTelemetry` | class | `Telemetry/TraceTests.cs` | `TraceTests.Sut_spans_nest_under_the_test_and_carry_the_test_id` | parent = test body span |
| exported `test case` span with `test.case.result.status = pass`, status `Unset` | engine | — | `Telemetry/TraceTests.cs` | `TraceTests.Dependency_exported_a_passed_test_case_span` | asserted from a `[DependsOn]` test |
| `TestContext.RegisterTrace(ActivityTraceId)` | `TUnit.Core` | method | `Telemetry/TraceTests.cs` | `TraceTests.External_traces_can_be_linked` | links out-of-process traces into the HTML report |
| `AddInMemoryExporter(ICollection<Activity>)` with a lock-guarded collection | `OpenTelemetry.Trace` | method | `Telemetry/TraceSetup.cs` | `TraceTests` | tests export concurrently |
| `ILoggingBuilder.AddTUnit(TestContext)` (`TUnit.Logging.Microsoft`) | `TUnit.Logging.Microsoft` | method | `Telemetry/TraceTests.cs` | `LoggingTests.Microsoft_logging_is_bridged_into_the_test_output` | `ILogger` → `GetStandardOutput()` |
| `TestContext.GetDefaultLogger().LogInformation` / `GetStandardOutput()` | `TUnit.Core` | method | `Telemetry/TraceTests.cs` | `LoggingTests.Default_logger_reaches_output_and_custom_sinks` |  |
| `ILogSink` + `TUnitLoggerFactory.AddSink` (`[Before(TestDiscovery)]`) | `TUnit.Core.Logging` | interface, method | `Telemetry/TraceSetup.cs` | same (`Context` is the `TestContext`) | `TUnit.Core.Context` clashes with a `Context` namespace — qualify it |
| `TestContext.GetById(id)` + `MakeCurrent()` under `ExecutionContext.SuppressFlow()` | `TUnit.Core` | method | `Telemetry/TraceTests.cs` | `LoggingTests.Make_current_reattaches_output_from_a_foreign_context` | cross-thread output correlation |
| `MetricCollector<T>` / `FakeLogger<T>` / OpenTelemetry in-memory metric exporter | `Microsoft.Extensions.Diagnostics.Metrics.Testing` / `Microsoft.Extensions.Logging.Testing` / `OpenTelemetry.Metrics` | class | `Telemetry/SignalsTests.cs` | `SignalsTests` | framework-neutral |
| `[LoggerMessage]` + `ActivitySource` + `Meter` in the SUT | `Microsoft.Extensions.Logging` / `System.Diagnostics` | — | `Telemetry/OrderService.cs` | all telemetry tests | tests share `[NotInParallel(Telemetry.Key)]` |

### ASP.NET Core — `TUnit 1.68.17` + `TUnit.AspNetCore`

| API | Namespace | Kind | Example | Verified by | Note |
|---|---|---|---|---|---|
| `TestWebApplicationFactory<TEntryPoint>` | `TUnit.AspNetCore` | class | `TUnit 1.68.17/TUnit 1.68.17/WebApplicationFactory.cs` | `Tests.Test`, `TracedWebTests` | replaces vanilla `WebApplicationFactory` (analyzer TUnit0064) |
| `WebApplicationTest<TFactory, TEntryPoint>` (`Factory`, `Services`, `UniqueId`, `GetIsolatedName`, `GetIsolatedPrefix`) | `TUnit.AspNetCore` | class | `TUnit 1.68.17/TUnit 1.68.17/TracedWebTests.cs` | `TracedWebTests.Isolation_helpers_are_available_on_the_base_class` | per-test isolated factory |
| `traceparent` / `X-TUnit-TestId` propagation from `Factory.CreateClient()` | `TUnit.AspNetCore` | — | `TUnit 1.68.17/WebApp/Program.cs` (`/trace`) | `TracedWebTests.Server_sees_the_test_trace_and_test_id` | server `Activity.TraceId` == test `TraceId`; header == `TestContext.Id` |
| server-side `ILogger` routed into the calling test | `TUnit.AspNetCore` | — | `TUnit 1.68.17/WebApp/Program.cs` | `TracedWebTests.Server_side_logs_are_routed_into_this_test` | `CorrelatedTUnitLoggerProvider` + `TUnitTestContextMiddleware` |
| `WebApplicationTestOptions.EnableHttpExchangeCapture` + `HttpExchangeCapture.Last` | `TUnit.AspNetCore` / `TUnit.AspNetCore.Interception` | property, class | `TUnit 1.68.17/TUnit 1.68.17/TracedWebTests.cs` | `TracedWebTests.Http_exchanges_are_captured_for_assertions` | resolve the store from `Services` (see divergences) |

### Reporting — `TUnit.Patterns.Policies 1.68.17`

| API | Namespace | Kind | Example | Verified by | Note |
|---|---|---|---|---|---|
| `BeforeTestDiscoveryContext.Settings.Reporting` (`ReportingSettings`) | `TUnit.Core.Settings` | class, property | `TUnit.Patterns.Policies 1.68.17/ReportingPolicy.cs` | `ReportingPolicyTests.Discovery_hook_configured_the_reports` | set in `[Before(TestDiscovery)]` |
| `ReportingSettings.HtmlReportEnabled` / `JsonReportEnabled` | `TUnit.Core.Settings` | property | same | `TestResults/` after a run holds `…-report.html` and no `….tunit-report.json`; a stale sidecar is deleted | `TUNIT_DISABLE_HTML_REPORTER` / `TUNIT_DISABLE_JSON_REPORT` take precedence |
| `ReportingSettings.ArtifactUploadEnabled` | `TUnit.Core.Settings` | property | same | `ReportingPolicyTests` | CI artifact upload only; `TUNIT_DISABLE_ARTIFACT_UPLOAD` takes precedence |

### Native AOT

| API | Namespace | Kind | Example | Verified by | Note |
|---|---|---|---|---|---|
| `<PublishAot>true</PublishAot>` on a TUnit project | MSBuild | config | `TUnit.Patterns.Policies 1.68.17/TUnit.Patterns.Policies 1.68.17.csproj` | `dotnet publish -c Release -r osx-arm64`, then run the native executable | source-generated mode needs no reflection |

### Divergences from docs (verified on 1.68.17)

- `ITestRetryEventReceiver.OnTestRetry` is declared in `TUnit.Core` but nothing in the engine invokes it; read `Execution.RetryAttempts` / `CurrentRetryAttempt` instead.
- `[AssertionFrom]` does not substitute `{parameter}` placeholders in `ExpectationMessage` (only `[GenerateAssertion]` does); `nameof(string.StartsWith)` also fails to generate because of its overloads — point it at a single-overload static helper.
- `TestContext.GetDisplayName()` does not exist; use `TestContext.Current.Metadata.DisplayName`.
- `EventReceiverStage.Early` moves the **end** receiver before `[After(Test)]` hooks as well, not only the start receiver before `[Before(Test)]`.
- An assembly-level `[ParallelLimiter<T>]` replaces class- and method-level ones on the pinned version; keep assembly-wide policies in their own test project.
- `TUnit.OpenTelemetry` auto-start stays dormant when any listener is attached to the `TUnit` source, and the built-in HTML reporter always is one; `TUNIT_OTEL_AUTOSTART=1` (set before the `Order = int.MaxValue` hook runs) forces it.
- `InconclusiveTestException` is reported as a failed test, not an inconclusive one.
- `WebApplicationTest.HttpCapture` returns a store the capture middleware never writes to; the populated `HttpExchangeCapture` is the one registered in the SUT's services.
- `Assert.That(typeof(X))` uses the represented type only for the first assignability assertion; after `.And` / `.Or` the check runs against the `RuntimeType` object (stated in the XML docs of `TypeValueAssertion`, not in the assertion docs).
