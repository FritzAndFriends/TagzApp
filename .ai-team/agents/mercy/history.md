# Mercy — History

## Project Context
- **Project:** TagzApp — .NET 9.0 Blazor Server app for social media hashtag aggregation
- **Stack:** C#, .NET 9, Blazor Server/WASM, SignalR, Entity Framework, PostgreSQL/SQLite, .NET Aspire
- **User:** Jeffrey T. Fritz (Jeff)
- **Focus:** LinkedIn provider integration, static event display screen, broader usability
- **Existing providers:** Twitter/X, Mastodon, Bluesky, YouTube, TwitchChat, Blazot, AzureQueue
- **Key projects:** TagzApp.Blazor (server), TagzApp.Blazor.Client (WASM), TagzApp.Common (shared models), TagzApp.Communication (SignalR)
- **Provider pattern:** Each provider implements ISocialMediaProvider, has its own project (TagzApp.Providers.{Name})
- **Tests:** xUnit in TagzApp.UnitTest, Playwright in TagzApp.WebTest
- **Build:** `cd src && dotnet restore && dotnet build TagzApp.Blazor`
- **Format:** `cd src && dotnet format --exclude **/Migrations/` (required before commit)

## Learnings

### 2026-02-18: Provider Architecture Pattern
- Every provider follows a strict 4-file pattern: `{Name}Configuration.cs` (implements `IProviderConfiguration`), `{Name}Provider.cs` (implements `ISocialMediaProvider`), `Start{Name}.cs` (implements `IConfigureProvider`), plus a Blazor config UI `.razor` file.
- The core interfaces live in `TagzApp.Common.Client` (`ISocialMediaProvider`, `IProviderConfiguration`) and `TagzApp.Common` (`IConfigureProvider`).
- Providers are registered in `src/TagzApp.Blazor/Service_Providers.cs` — a static list of `IConfigureProvider` instances.
- Config UI components are in `src/TagzApp.Blazor.Client/Components/Admin/` and wired via a switch statement in `src/TagzApp.Blazor/Components/Admin/Pages/GenericProvider.razor` matching on provider type name.
- Provider icon mapping lives in `src/TagzApp.ViewModels/Data/ContentModel.cs` method `MapProviderToIcon`. Default case auto-maps to `bi-{provider}`.
- Bluesky uses a WebSocket firehose (real-time). Mastodon uses HttpClient polling with `HttpClientOptions`. For polling-based providers, Mastodon is the better structural reference.
- Configuration key convention is `provider-{PROVIDER_ID}` (e.g., `provider-BLUESKY`, `provider-mastodon`).
- Project references: provider `.csproj` references `TagzApp.Common` and `TagzApp.Communication`. The main `TagzApp.Blazor.csproj` references all provider projects.
- Target framework in providers is `net10.0` (matching repo convention).
- `BaseProviderConfiguration<T>` exists in Common.Client for the newer reactive `IOptionsMonitor<T>` pattern, but not all providers use it yet. Bluesky still uses the simpler direct `IProviderConfiguration` style.

### 2026-02-18: LinkedIn Provider Plan Created
- Decision document written to `.ai-team/decisions/inbox/mercy-linkedin-provider-plan.md`
- LinkedIn's Marketing API is the only viable option for hashtag search; requires MDP partner approval (2-6 weeks)
- Rate limits are very restrictive: 100 calls/day per member token
- Recommended polling interval: 5-15 minutes (conservative)
- No third-party SDK needed — HttpClient is sufficient
- Provider ID will be `LINKEDIN`, config key `provider-LINKEDIN`
- Work broken into 4 phases, 10 tasks, estimated 3-4 days for Sombra

## Team Updates
- 📌 **Team update (2026-02-19)**: global.json rollForward=latestMinor is Jeff's intentional setting. NEVER modify global.json under any circumstances.
