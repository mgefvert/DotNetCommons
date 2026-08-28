# DotNetCommonTests.EF project context

## Purpose and coverage

MSTest/FluentAssertions coverage for `DotNetCommons.EF`, targeting `net10.0` with coverlet collection.

- `PatchTests` covers reflected scalar/collection patch behavior.
- `TestDataReaderTests` maps Markdown table data under `TestData` into `Account`/`Customer` fixtures.
- `Assembly.cs` contains suite-level MSTest configuration.

## Current state and verification

Initial scan: 2026-08-23; no work is in progress. A no-restore solution build compiled this test assembly successfully. Run `dotnet test DotNetCommons/test/DotNetCommonTests.EF/DotNetCommonTests.EF.csproj` after EF changes; extend coverage for CRUD operations, seeding, DbSet helpers, and concurrent-list behavior when those areas change.
