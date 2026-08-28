# Fleet example context

## Purpose

A `net10.0` executable demonstration of the core `CommandActionRegistry`, DI, async/sync actions, command routing, option models, lifecycle hooks, wildcard selection, logging, and persisted application state. Its assembly name is `fleet`.

## Architecture

- `Program.cs` configures Serilog through Microsoft logging, registers `UnitStates`, loads `unitStates.json`, registers attributed actions, logs before/after each action, executes, and saves state in `finally`.
- Actions implement deploy/recall, set/list DEFCON and REDCON, list units, nuclear targeting, and the composite `broken arrow` flow.
- `UnitArgs`, `UnitValueArgs`, and `NukeArgs` demonstrate inherited command-line option models. `UnitStates.SelectUnits` supports wildcard matching.

## Constraints and state

- This project is executable documentation; keep it readable and representative of recommended command-action usage.
- Runtime state is written to `unitStates.json` in the working directory.
- Initial scan: 2026-08-23; no work is in progress. The no-restore solution build compiled this example successfully.

## Verification

Build `DotNetCommons/examples/Fleet/Fleet.csproj`, then exercise a read-only/list route or run command-registry tests in the core test project. Update `README.md` when command routes/options change.
