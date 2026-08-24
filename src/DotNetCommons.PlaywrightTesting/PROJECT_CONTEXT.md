# DotNetCommons.PlaywrightTesting project context

## Purpose

A custom browser-testing layer over Microsoft Playwright targeting `net10.0`. It references core `DotNetCommons`, AsyncEnumerator, and FluentAssertions.

## Main components

- Attributes identify test classes, tests, class/test setup and teardown, and execution order (`First`, `Single`, `Parallel`, `Last`).
- `PlaywrightSession` owns Playwright/browser lifetime and creates contexts. `PlaywrightContext` owns browser contexts/pages and can run work in new pages or in parallel.
- `PlaywrightPage` wraps locators/page actions with state assertions, polling/await helpers, navigation, typing/clicking/checking, URL/text checks, and screenshot capture.
- `TestRunner` discovers attributed public classes by reflection, invokes lifecycle methods, runs parallel tests with a maximum degree of three, and accumulates `TestResult` records.
- `ScreenShotHelper` normalizes output filenames; `PlaywrightTestingException` reports framework-level failures.

## Constraints and known questions

- Async-disposable ownership is important: session owns browser, context owns pages, and runner-created instances follow attribute lifecycles.
- There is no dedicated test project for this library. Add non-browser unit coverage for discovery/lifecycle logic and explicit browser integration coverage for wrappers when changing them.
- `TestRunner.Run` currently returns `!Results.Any()`, which is false whenever any test result exists, regardless of success. Confirm intended semantics before relying on its Boolean return or changing it.
- Initial scan: 2026-08-23; no work is in progress. Full no-restore verification was blocked because project assets are absent locally.

## Verification

Restore packages, install the required Playwright browser binaries, then build the project and run a small attributed test assembly against a controlled local site.
