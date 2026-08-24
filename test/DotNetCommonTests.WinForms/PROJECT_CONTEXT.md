# DotNetCommonTests.WinForms project context

## Purpose and coverage

Windows-only MSTest/FluentAssertions coverage for `DotNetCommons.WinForms`, targeting `net10.0-windows` with WinForms enabled and coverlet collection.

Tests cover `BitmapBuffer`, bitmap processing, and `ExifImage`; `Graphics/test.jpg` is embedded as deterministic EXIF/image input. Tests that allocate images or lock buffers must dispose them even on failure.

## Current state and verification

Initial scan: 2026-08-23; no work is in progress. The full no-restore build could not compile this project because its NuGet asset file is absent locally. On Windows, restore and run `dotnet test DotNetCommons/test/DotNetCommonTests.WinForms/DotNetCommonTests.WinForms.csproj`. Service manager, P/Invoke, app-bar, elevation, hotkey, and form behavior currently lack automated coverage and may require guarded integration tests.
