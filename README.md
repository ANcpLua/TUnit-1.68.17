# TUnit 1.68.17 showcase

Runnable examples of TUnit's current API on .NET 10. Each example asserts the behavior it teaches.
The [API catalog](API.md) connects features to their source files and tests.

| Project | Examples |
|---|---|
| `TUnit.Mixed 1.68.17` | Basic tests, data sources, lifecycle hooks, dependency injection, and cancellation |
| `TUnit.Patterns 1.68.17` | Custom assertions, fixtures, executors, ordering, parallelism, and telemetry |
| `TUnit.Patterns.Policies 1.68.17` | Assembly policies, reporting configuration, and Native AOT |
| `TUnit.Mocks 1.68.17` | Typed setups, verification, events, wrapping, and internal SDK types |
| `TUnit 1.68.17` | ASP.NET Core testing, HTTP capture, and correlated logs and traces |
| `TUnit.Playwright 1.68.17` | Browser tests, independent contexts, and video artifacts |
| `AdvancedPatterns` | Reusable contracts, protocol hosts, workflows, and file-store checks |

Build the solution, then run a project:

```sh
dotnet build "TUnit 1.68.17.slnx"
dotnet run --no-build --project "TUnit.Patterns 1.68.17/TUnit.Patterns 1.68.17.csproj"
dotnet run --no-build --project "TUnit.Mocks 1.68.17/TUnit.Mocks 1.68.17/TUnit.Mocks 1.68.17.csproj"
```

Playwright installs its browsers on the first run and includes a test that visits playwright.dev.
AdvancedPatterns' two live Anthropic tests skip without `ANTHROPIC_API_KEY`; running them with a
key uses the account's API credits. `[Explicit]` examples demonstrate deliberate failures,
cancellation, or timeouts and are selected separately with `--treenode-filter`.

Native AOT:

```sh
dotnet publish "TUnit.Patterns.Policies 1.68.17/TUnit.Patterns.Policies 1.68.17.csproj" -c Release -r osx-arm64 -o artifacts/publish/policies
"artifacts/publish/policies/TUnit.Patterns.Policies 1.68.17"
```

Verified on 2026-09-23 with .NET SDK 10.0.401 on macOS arm64:

| Project | Passed | Skipped |
|---|---:|---:|
| Mixed | 29 | 0 |
| Patterns | 203 | 2 |
| Policies | 11 | 0 |
| Mocks | 22 | 0 |
| ASP.NET Core | 5 | 0 |
| Playwright | 6 | 0 |
| AdvancedPatterns | 10 | 2 |
| **Total** | **286** | **4** |

The solution build completed with no warnings or errors. The two AdvancedPatterns live-service
tests were skipped with `ANTHROPIC_API_KEY` unset. The Policies native executable passed all
11 tests. The explicit dependency-failure, inconclusive, cancellation, and timeout examples
were run separately and produced their documented outcomes.
