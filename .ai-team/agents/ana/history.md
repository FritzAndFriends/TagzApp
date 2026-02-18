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
- Test files live in subdirectories by provider: `src/TagzApp.UnitTest/LinkedIn/LinkedInProviderTests.cs`
- Provider tests use `StubHttpMessageHandler` + `StubHttpClientFactory` for HTTP mocking (no external packages needed)
- `LinkedInProvider` constructor takes `(IHttpClientFactory, LinkedInConfiguration)` — follows Mastodon pattern
- `LinkedInConfiguration` implements `IProviderConfiguration` with switch-based `GetConfigurationByKey`/`SetConfigurationByKey`
- Key domain types: `Content`, `Creator`, `Hashtag`, `SocialMediaStatus` (all in `TagzApp.Common.Models` / `TagzApp.Common`)
- `SocialMediaStatus` enum: Disabled=-2, Unknown=-1, Unhealthy=0, Degraded=1, Healthy=2
- Provider health tests check token expiry thresholds: >7 days=Healthy, <7 days=Degraded, expired=Unhealthy
- Daily call budget tests: exhaust budget → `GetContentForHashtag` returns empty, `GetHealth` returns Degraded
- Global using `Xunit` is in `_Usings.cs`; `TagzApp.Common` and `TagzApp.Common.Models` are in the csproj `<Using>` items

## Team Updates
- 📌 **2026-02-18**: LinkedIn provider plan decided by Mercy — architecture approved for implementation. 10 work items across 4 phases (scaffolding, core provider, integration, testing/docs). Estimated 2-3 days for implementation, 1 day for tests/docs.
- 📌 **2026-02-18**: LinkedIn Provider Unit Tests completed. 32 test cases written covering configuration, content mapping, budget tracking, token expiry, metadata. Aligned with Sombra's implementation. All tests pass. Full test suite: 64 passed, 2 skipped, 0 failed. Commit: 2da5f39 — decided by Ana
