# DotNetCommons.WinForms project context

## Purpose

Windows-only WinForms, drawing, and Win32 helpers targeting `net10.0-windows`. Unsafe code is enabled for pixel-buffer operations. Tests live in `test/DotNetCommonTests.WinForms`.

## Main components

- Form/control helpers: layered alpha forms, app-bar registration, double buffering, row selection, hotkeys, elevation/relaunch, and natural string sorting.
- Graphics: bitmap creation/crop/resample/fit/cover/thumbnail/stream output, locked `BitmapBuffer` and scan-line pixel/alpha operations, background removal, EXIF read/write/orientation, and native GDI text rendering.
- Windows services: SCM/service safe-handle-style wrappers for install/open/start/stop/status/config/uninstall flows.
- `WinApi*` centralizes P/Invoke constants, enums, structs, and functions used by the higher-level wrappers.

## Constraints and state

- Keep pointer arithmetic, bitmap lock/unlock, GDI handles, service handles, and form registration balanced through `Dispose`/unregister paths.
- Windows platform behavior and pixel formats are part of the API; avoid assuming cross-platform `System.Drawing` support.
- Initial scan: 2026-08-23; no work is in progress. Full no-restore verification was blocked because WinForms project assets are absent locally.

## Verification

On Windows, restore and run `dotnet test DotNetCommons/test/DotNetCommonTests.WinForms/DotNetCommonTests.WinForms.csproj`. Current tests cover bitmap buffers/processing and EXIF using an embedded JPEG.
