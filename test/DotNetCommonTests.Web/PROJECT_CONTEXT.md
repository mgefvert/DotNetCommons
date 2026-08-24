# DotNetCommonTests.Web project context

## Purpose and coverage

MSTest/FluentAssertions coverage for `DotNetCommons.Web`, targeting `net10.0` with coverlet collection. Newtonsoft.Json is test-only support.

Tests cover the mutable HTML node model (`HAttribute`, comments, elements, nodes, tags, and text), `HtmlParser`, and paginator construction/rendering. Assertions protect escaping, tree mutation/traversal, tag generation, parsing, and page-link behavior.

## Current state and verification

Initial scan: 2026-08-23; no work is in progress. A no-restore solution build compiled this test assembly successfully. Run `dotnet test DotNetCommons/test/DotNetCommonTests.Web/DotNetCommonTests.Web.csproj`; add focused tests when changing middleware, MVC rendering/controller helpers, menus, or other HTML utilities currently outside the suite.
