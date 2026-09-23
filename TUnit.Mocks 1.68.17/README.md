# TUnit.Mocks — source-generated mocking

[TUnit.Mocks](https://tunit.dev/docs/writing-tests/mocking/) generates mocks at compile time.
`IGreeter.Mock()` works as a C# 14 static extension whose wrapper implements the interface.
Setups, matchers, and verification share one typed surface —
`mock.Greet(Any()).Returns("hi")` to stub, `mock.Greet("Alice").WasCalled(Times.Once)` to verify —
with matchers imported globally.

- `TUnit.Mocks 1.68.17/MockEssentials.cs` — first mock, loose-mode smart defaults, inline lambda
  matchers, argument capture, sequential `.Then()` setups, strict mode.
- `TUnit.Mocks 1.68.17/StatefulConnectionTests.cs` — state-machine mocking (`InState` /
  `TransitionsTo`), typed event raising, subscription tracking.
- `TUnit.Mocks 1.68.17/PendingTaskTests.cs` — async return factories, pending calls, and a
  caller's timeout while waiting for a result.
- `TUnit.Mocks 1.68.17/RuntimeAutoStubTests.cs` — loose mocks provide runtime stubs for
  SDK-internal interfaces without a generated mock. This fallback requires dynamic code;
  it is separate from the source-generated mocks used by the other examples.
- `TUnit.Mocks 1.68.17/InternalsAccessTests.cs` — configure internal SDK types through
  experimental internals access. Opt in with `<TUnitMocksExperimentalInternalsAccess>` and
  `<TUnitMocksInternalsAccess Include="VendorSdk"/>` in the test project's csproj.
- `TUnit.Mocks 1.68.17/InitOnlyMemberTests.cs` — configure `init` properties and indexers;
  an unconfigured virtual property retains its base value.
- `TUnit.Mocks 1.68.17/WrappedInstanceTests.cs` — wrap a real instance, call its implementation,
  and verify the recorded call through `Mock.Wrap(instance)`.
- `TUnit.Mocks 1.68.17/StaticAbstractTests.cs` — mock the instance members of an interface
  with `static abstract` members, then pass `.Object` to the caller.
- `TUnit.Mocks 1.68.17/RefStructEventTests.cs` — raise events with stack-only or by-reference
  arguments. Create stack-only arguments inside `.Callback(...)` when a setup raises them.
- `VendorSdk/` — a small SDK stand-in used by the runtime-stub and internals-access examples.

Run it with:

```bash
dotnet run --project "TUnit.Mocks 1.68.17/TUnit.Mocks 1.68.17/TUnit.Mocks 1.68.17.csproj"
```
