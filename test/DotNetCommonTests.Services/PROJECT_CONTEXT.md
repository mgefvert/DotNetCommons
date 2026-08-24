# DotNetCommonTests.Services project context

## Purpose and coverage

MSTest/FluentAssertions coverage for `DotNetCommons.Services`, targeting `net10.0` with coverlet. EF Core SQLite provides an isolated relational store for queue tests.

- `JobQueueServiceTests` exercises worker/job state transitions and maintenance against SQLite.
- Email/SMS configuration tests cover allow-list behavior.
- Debug integration tests cover non-production email/SMS implementations; keep them isolated from real providers.

## Current state and verification

Initial scan: 2026-08-23; no work is in progress. The full no-restore build could not compile this project because its NuGet asset file is absent locally. Restore, then run `dotnet test DotNetCommons/test/DotNetCommonTests.Services/DotNetCommonTests.Services.csproj`. Use injected/fake time for queue timing and never add tests that contact SMTP or Spirius by default.
