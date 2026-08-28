# DotNetCommons.EF project context

## Purpose

Reusable Entity Framework Core patterns targeting `net10.0`. The project references the core `DotNetCommons` assembly and Microsoft EF Core abstractions. Tests live in `test/DotNetCommonTests.EF`.

## Main components

- `ObjectManagement/Patch` reflects `[Patch]` properties and updates scalar objects or keyed collections. `PatchMode` controls whether collection members may be created or removed; `IValidation` supplies validation hooks.
- `EfCore/CrudOperations` is the current configurable CRUD implementation. It provides create/get/list/update/delete flows with selectors, patching, access checks, query shaping, and cancellation.
- `ICrudService` and `CrudService` are obsolete compatibility APIs. Do not use them for new work; Web still consumes the interface.
- `DbSetExtensions` supports seeding and expression-based property lookup/collection predicates.
- `DbConcurrentList` caches database-backed keyed records and coordinates asynchronous get-or-add; the discriminator type selects cache partitions.
- `EfDateOnlyConverter` maps `DateOnly` for EF.
- `DataSeeder` discovers `IReferenceSeeder`/`ITestSeeder` implementations on a context; `DbContextSeeder` supports database creation and enum-to-reference-table records.
- `TestDataReader` loads tabular Markdown test data and maps it to objects.

## Constraints and state

- Keep database operations provider-neutral here; MySQL-specific setup belongs in consumers.
- Reflection-based patching depends on explicit attributes and keys. Preserve partial-update semantics and test null/create/remove cases.
- Initial scan: 2026-08-23; no work is in progress. A no-restore solution build compiled this project and its tests successfully.

## Verification

Run `dotnet test DotNetCommons/test/DotNetCommonTests.EF/DotNetCommonTests.EF.csproj` after changes. Current tests cover patch behavior and Markdown test-data reading; add focused tests for new EF helpers.
