# Ana — History

## Project Context
- **Project:** TagzApp — .NET 9.0 Blazor Server app for social media hashtag aggregation
- **Stack:** C#, .NET 9, Blazor Server/WASM, SignalR, Entity Framework, PostgreSQL/SQLite, .NET Aspire
- **User:** Jeffrey T. Fritz (Jeff)
- **Focus:** LinkedIn provider integration, static event display screen, broader usability
- **Test project:** TagzApp.UnitTest (xUnit), TagzApp.WebTest (Playwright)
- **Test run:** `cd src/TagzApp.UnitTest && dotnet test --verbosity normal`
- **Expected results:** ~22 total tests, ~18 pass, ~2 fail (Mastodon external API), ~2 skip (YouTube missing keys)
- **Format required:** `cd src && dotnet format --exclude **/Migrations/` before committing
- **Build:** `cd src && dotnet restore && dotnet build TagzApp.Blazor`

## Learnings

## Team Updates
- 📌 **2026-02-18**: LinkedIn provider plan decided by Mercy — architecture approved for implementation. 10 work items across 4 phases (scaffolding, core provider, integration, testing/docs). Estimated 2-3 days for implementation, 1 day for tests/docs.
