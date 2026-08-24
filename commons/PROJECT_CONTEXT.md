# commons CLI project context

## Purpose

The `commons` executable is an internal maintenance/data-import CLI targeting `net10.0`. It composes core command actions, Services, SqlData, MySQL EF Core, BCrypt, and CsvHelper. It is a consumer/tool, not a reusable library.

## Commands and flow

- `Program.cs` loads `appsettings.json`, configures common console logging and DI, captures the current `Invocation`, resolves MySQL login paths through `MySqlCnfReader`, registers attributed commands, and executes the selected route.
- `dev-clean` cleans development artifacts; `gen-pw` generates password material.
- `import-geo-*` imports airports, countries, area codes, and ZIP data; `import-geo-all` orchestrates them.
- `import-ip` imports IP geolocation boundaries; `test-ip` queries the resulting data.
- `Helper` creates configured CSV readers and downloads/cache source files. Connection-bearing commands share `ConnectionArgs` and require `-c|--connection`.

## Constraints and state

- Commands can download external datasets and mutate a configured MySQL database. Verify the selected login path/database before running imports; never treat imports as harmless tests.
- `appsettings.json` is copied to output and may describe external sources/settings. Do not commit secrets.
- Initial scan: 2026-08-23; no work is in progress. Full no-restore verification was blocked because this project's asset file is absent locally.

## Verification

After restore, build `DotNetCommons/commons/commons.csproj`; exercise help or a non-destructive command before database imports. There is no dedicated automated test project.
