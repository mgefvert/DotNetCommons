# DotNetCommonTests project context

## Purpose and coverage

The main MSTest/FluentAssertions suite for core `DotNetCommons`, targeting `net10.0` with coverlet collection. Folders mirror source domains.

Coverage includes AI vectors; check digits; collections; colors; command discovery, parsing, resolution, scheduling, and hooks; compression; networking; numerics; security and both crypto versions; state machines; process/command-line helpers; synchronization; holidays/date utilities; Aho-Corasick, CSV, fixed-width formats, parsers, shunting-yard evaluation, text tools/tokenization; and most extension classes.

## Conventions and gaps

- Tests are MSTest (`[TestClass]`, `[TestMethod]`) with assembly-level parallelization settings in `Assembly.cs` and FluentAssertions for readable assertions.
- Keep tests deterministic. Time-sensitive logic should use `FakeTimeProvider`; process tests must clean up spawned processes; crypto tests must not embed production secrets.
- Some core surfaces have little/no direct coverage, including Ollama network calls, logging UI output, several console/system helpers, vCards/cookie persistence, and parts of access caching.

## Current state

Initial scan: 2026-08-23; no work is in progress. A no-restore solution build compiled this test assembly successfully.

## Verification

Run `dotnet test DotNetCommons/test/DotNetCommonTests/DotNetCommonTests.csproj`. Add tests in the matching source-area folder whenever core behavior changes.
