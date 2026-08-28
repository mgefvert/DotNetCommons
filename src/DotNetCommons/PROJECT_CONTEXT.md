# DotNetCommons project context

## Purpose and boundaries

`DotNetCommons` is the dependency-light, cross-platform core library for genuinely reusable helpers. It targets `net10.0` with nullable reference types and implicit usings enabled. Its only packages are Microsoft logging and dependency-injection abstractions/console support. Framework-specific code belongs in a sibling module, and every addition must have unit coverage in `test/DotNetCommonTests`.

The public API is broad and mature. Prefer an existing helper over adding a near-duplicate, and preserve current behavior unless tests and consumers are deliberately updated.

## Architecture and major APIs

- **Results and errors:** `Result<T>`, `Results<T>`, `Error`, and `AppException` model success/failure and user-facing HTTP status errors. `LittleStateMachine<T>` implements configured states, parent states, transitions, and arrival/departure hooks.
- **Commands:** `CommandAction`, `CommandAction<TArgs>`, `CommandActionAttribute`, and `CommandActionRegistry` provide DI-created, attributed CLI actions. The registry discovers/registers assemblies or commands, resolves routes, parses options, supports default/scheduled commands, and exposes before/after invocation, action, and help hooks. `DotNetCommonsCommandLineParser` is the parser used by the registry; `Sys/CommandLine*` is an older attribute-based parser/toolset.
- **AI:** `FloatVector` and `DoubleVector` provide dot product, cosine similarity, magnitude, normalization, arithmetic operators, conversion, and JSON converters. `OllamaClient` requests local/remote embeddings.
- **Collections:** `CircularBuffer<T>` is a fixed-capacity FIFO; `CssClassList` maintains normalized CSS tokens; `DrawList<T>` provides seeded weighted/random draws; `Grid<TRow,TCol,TData>` stores sparse two-dimensional data, inverts/manipulates it, and renders CSV, HTML, or Markdown.
- **Colors:** RGB, grayscale, HSB, HSL, and Oklab models convert through `ColorConversion`; RGB also supports mixing, lightening/darkening, hex, byte colors, and `System.Drawing.Color` conversion.
- **Check digits and numerics:** Luhn and ABA calculation/append/validation; factor discovery, delimited sequences, ordinals, Roman numerals, an LCG randomizer, and `Federated48Key` composition/splitting.
- **IO and networking:** compression/decompression (Deflate, GZip, Brotli), console progress bars, cookie-container serialization, IP allow/deny lists, URI query construction, and vCard parsing/rendering.
- **Security:** claims lookup, CRC32, password generation/hash/verification, sanitization, TOTP, MySQL login-path reading, legacy stream crypto (`CryptV1`), and AES-GCM/key derivation (`CryptV2`). Cryptographic key types are disposable and clear sensitive buffers.
- **Synchronization/system:** `Accessor<T>` and `AccessCache<TKey,TValue>` provide synchronized replace/get/cache flows. Clock jobs, process spawning, console menus/color, Ctrl-Break handling, and command-line processing live under `Sys`.
- **Temporal:** date ranges/approximations, fake time, time-change waiting, date generation/time-zone lookup, and extensible holidays (Swedish, US, Easter, date-, weekday-, and relative-date rules).
- **Text:** Aho-Corasick matching, wildcard-to-regex matching, natural sort, CSV mapping, fixed-width record attributes/conversion, Apache/config parsers, shunting-yard evaluation, configurable tokenization, encoding/UTF-8 checks, ASCII folding, Levenshtein distance, and wrapping/splitting helpers.

## Extension helper inventory

- `CommonCollectionExtensions`: `Deconstruct`, conditional/range add, `Batch`, extract by index/predicate/range/first/last, `ForEach`, dictionary `Increment`, collection intersection, cardinality checks (`IsEmpty`, `IsOne`, `IsAtLeastOne`, `IsMany`), `Join`, one/many relationship linking, `MinMax`, null filtering, random order/repeat/swap, hex encoding, random `Toss`, tree walking, cyclic item lookup, and indexed enumeration.
- `CommonDateOnlyExtensions`: starts/ends of week, month, and year; inclusive range checks; OLE Automation day; day replacement; `DateTime` conversion; ISO-8601 formatting.
- `CommonDateTimeExtensions`: age in several units, starts/ends of time periods, Unix second/millisecond conversions for `DateTime` and `DateTimeOffset`, proximity/range checks, OLE Automation day, day replacement, `DateOnly` conversion, ISO-8601 formatting, and truncation.
- `CommonStringExtensions`: line breaking/chomping/newline conversion, ordinal-insensitive contains/equality, first-line and null/empty helpers, left/mid/right extraction, ellipsis and masking, whitespace normalization, boolean/numeric parsing, repetition, initial-letter casing, and trim/filter. Companion files add general case conversion and delimited sub-item add/get/count/insert/remove/set.
- `CommonFileInfoExtensions`: derive sibling file/directory paths, create/touch, and read/write text or bytes.
- `CommonIPAddressExtensions`: IPv4/IPv6 conversion to/from unsigned 32-, 64-, and 128-bit integers.
- `CommonPropertyInfoExtensions`: default-value detection and reflected property/value conversion and assignment.
- Other focused extensions: detailed exception text; hash-to-string computation; common HTTP GET/POST deserialization; normal random generation; comparable `Between`/`Limit` and parity checks; type ancestry/nullability/numeric checks; URI query replacement; `TimeProvider` helpers; console helpers; and claims accessors.

## Implementation conventions and constraints

- Most tests use MSTest plus FluentAssertions. Test folders mirror source areas; add or update focused tests with behavior changes.
- Several APIs are intentionally mutable and fluent. Check return types and existing tests before changing them to immutable patterns.
- Date/time code mixes `DateTime`, `DateTimeOffset`, `DateOnly`, and `TimeProvider`; preserve kind/UTC semantics explicitly.
- `Sys.Spawn` owns process lifetime and redirected streams; use its async/cancellation paths for long-running processes.
- `CryptV1` is retained for compatibility. Prefer `CryptV2` for new encrypted payloads.

## Current state and verification

- Initial scan: 2026-08-23. No implementation work is in progress.
- `dotnet build DotNetCommons/DotNetCommons.slnx --no-restore` compiled this project successfully with .NET SDK 10.0.400. The full solution could not complete because other projects lacked restored asset files.
- The collection README is stale in places: projects target .NET 10, and advertised `IFileAccessor`/`JiwiConverter` APIs are not present in the source tree.

## Next steps

- Run `dotnet test DotNetCommons/test/DotNetCommonTests/DotNetCommonTests.csproj` after core changes.
- Keep this inventory updated when public helpers are added, removed, or substantially redefined.
