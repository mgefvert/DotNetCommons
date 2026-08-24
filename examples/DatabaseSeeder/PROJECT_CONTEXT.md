# DatabaseSeeder example context

## Purpose

A minimal `net10.0` console example for `DotNetCommons.EF.DataSeeding` with MySQL. It demonstrates a context implementing reference/test seeders and enum-backed reference rows.

## Flow

`Program.cs` reads `ConnectionStrings:Default` from copied `appsettings.json`, creates `MyDbContext`, calls `EnsureCreated`, then invokes `DataSeeder.SeedReferenceData` and `SeedTestData`. `MyDbContextSeeder` supplies the seed implementations for `AirportType` and sample `Airport` records.

## Constraints and state

- This is executable documentation, not production migration infrastructure. Repository policy does not use EF migrations.
- The configured database is created/modified when run. Keep credentials out of source and use a disposable database for demonstrations.
- Initial scan: 2026-08-23; no work is in progress. Full no-restore verification was blocked because the project asset file is absent locally.

## Verification

After restore, build the project. Running it requires a reachable MySQL database and intentionally changes that database.
