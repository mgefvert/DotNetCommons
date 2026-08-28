# DotNetCommons.Services project context

## Purpose

Reusable external integrations and a database-backed job queue targeting `net10.0`. It references core `DotNetCommons` and EF Core/Relational. Tests live in `test/DotNetCommonTests.Services`.

## Main components

- Email: `IEmailIntegration`, shared `AbstractEmailIntegration`, SMTP and debug implementations, configuration, and per-message batch results. Allowed-domain configuration gates recipients.
- SMS: `ISmsIntegration`, shared phone formatting, Spirius and debug implementations, configuration, message models, and batch results. Allowed-number configuration gates recipients.
- `IntegrationConfiguration.Require` validates named integration configuration. `IpifyIntegration` resolves the caller's public address.
- Job queue: `JobDbContext` and queue/type/worker/archive entities plus `IJobQueueService`/`JobQueueService`. Operations register/ping workers, enqueue/cancel/claim/complete/reschedule/fail jobs, report stats, archive closed jobs, evict stale workers, expire jobs, and reclaim abandoned work. Time is injected through `TimeProvider`; retries use exponential backoff.
- `Old` contains legacy service-context/identity/queue models. Treat it as compatibility code, not a pattern for new work.

## Constraints and state

- Debug integrations log instead of contacting providers and are the default test seam. Do not let tests send real email/SMS.
- Queue timestamps use the repository's UTC `...Z` convention. Preserve ownership and closed-job checks in every state transition.
- Initial scan: 2026-08-23; no work is in progress. Full no-restore verification was blocked because this project's NuGet assets are absent locally.

## Verification

Restore if needed, then run `dotnet test DotNetCommons/test/DotNetCommonTests.Services/DotNetCommonTests.Services.csproj`. Tests use SQLite for queue behavior and cover email/SMS configuration plus explicitly marked debug integration scenarios.
