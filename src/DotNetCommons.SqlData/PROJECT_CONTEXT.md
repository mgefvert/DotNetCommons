# DotNetCommons.SqlData project context

## Purpose

A MySQL-oriented geolocation lookup library targeting `net10.0`. It references the core and EF modules and uses EF Core for context management plus Dapper for lookup queries.

## Main components

- `SqlDataContext` maps IP lookup/country/city and geographic airport, area-code, country, and ZIP entities.
- `ISqlDataService`/`SqlDataService` expose city and country lookup for an `IPAddress`. IPv4 input is mapped to IPv6; Dapper queries find the greatest stored network boundary not exceeding the address.
- `SqlData.sql` is the schema for the lookup database. The sibling `commons` CLI downloads/imports source datasets into it.

## Constraints and state

- MySQL is authoritative. Raw SQL relies on binary IP ordering and MySQL hex literals; preserve parameter safety if queries are changed.
- This project has no dedicated test project. Changes should add isolated coverage where practical, especially around address-family normalization and boundary selection.
- Initial scan: 2026-08-23; no work is in progress. Full no-restore verification was blocked because this project's NuGet assets are absent locally.

## Verification

After restore, build `DotNetCommons/src/DotNetCommons.SqlData/DotNetCommons.SqlData.csproj`. Database integration verification requires a compatible MySQL schema/data set.
