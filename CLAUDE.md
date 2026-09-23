# TUnit showcase

This repository demonstrates the current pinned TUnit API with small, runnable examples.
`README.md` lists the projects and validation commands; `API.md` maps APIs to their examples.

## Examples

- Keep each example focused on an API a consumer would use, with assertions that demonstrate
  its behavior. Keep framework implementation probes and stress tests in the upstream suite.
- Write comments about current usage. Keep version comparisons, obsolete workarounds, and
  release history out of the examples and API catalog.
- Use `[Explicit]` for demonstrations that intentionally fail, cancel, or time out. Document
  their expected outcome so an ordinary run can remain successful.
- Preserve the distinction between source-generated mocks, runtime auto-stubs, and experimental
  internals access. Describe each example's requirements where it is listed.
- Add related examples to the owning project and update its README and the API catalog together.

## Package updates

1. Check release notes and version-matched source for the APIs demonstrated here.
2. Update every TUnit package reference, versioned path, namespace, and solution entry consistently.
3. Adapt examples to current usage and remove obsolete material. Update other package versions
   only when needed for compatibility.
4. Rebuild and run the affected examples before updating verification claims.

## Validation

- Build the solution with `dotnet build "TUnit 1.68.17.slnx"`.
- Run each test project using `dotnet run --no-build --project <csproj>`.
- For Native AOT, publish the Policies project with `-c Release -r osx-arm64` and run its executable.
- Run `[Explicit]` examples separately through `--treenode-filter`; their deliberate failures
  must match the documented behavior.
- Check that solution and README paths resolve and run `git diff --check` before publishing.

Only record counts observed on the pinned version, and distinguish focused runs from the whole
solution. External-service tests require explicit opt-in and report a skip when unavailable.
Keep local session notes, generated output, and unrelated sample projects outside the published tree.
