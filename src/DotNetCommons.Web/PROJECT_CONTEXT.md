# DotNetCommons.Web project context

## Purpose

ASP.NET Core helpers targeting `net10.0`, using the `Microsoft.AspNetCore.App` shared framework and referencing `DotNetCommons.EF`. Tests live in `test/DotNetCommonTests.Web`.

## Main components

- `Elements` is a small mutable HTML DOM: nodes, elements, attributes, text/comment nodes, tag factories, parsing, rendering/escaping, and `AppTagHelper` integration.
- `Html` contains pagination, menus, JSON debugging, Gravatar URLs, newline-to-`br`, color/CSS conversion, and light/dark color helpers.
- `AbstractCrudController` exposes conventional `create`, `get`, `list`, `update`, and `delete` endpoints around an `ICrudService`, with optional audit callbacks through `ICrudLogOperation`.
- `ControllerExtensions.RenderViewAsync` renders MVC views to strings.
- Middleware covers request logging, IP scoring/banning, and debugger breakpoints. `HttpStatusException` carries an HTTP status, and `SiteMapAttribute` marks endpoints for sitemap metadata.

## Constraints and known issues

- HTML mutation methods are fluent and rendering behavior is extensively tested; preserve escaping versus raw/style/script semantics.
- `AbstractCrudController` still uses obsolete `ICrudService`; the current no-restore build emits CS0618. Migration should be a deliberate cross-project change to the newer EF `CrudOperations` API.
- Initial scan: 2026-08-23; no work is in progress. The project and tests compile in the restored subset.

## Verification

Run `dotnet test DotNetCommons/test/DotNetCommonTests.Web/DotNetCommonTests.Web.csproj`. Existing coverage is concentrated on the HTML DOM/parser and paginator; middleware and controller helpers currently have no focused test files.
