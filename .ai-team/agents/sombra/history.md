# Sombra — History

## Project Context
- **Project:** TagzApp — .NET 9.0 Blazor Server app for social media hashtag aggregation
- **Stack:** C#, .NET 9, Blazor Server/WASM, SignalR, Entity Framework, PostgreSQL/SQLite, .NET Aspire
- **User:** Jeffrey T. Fritz (Jeff)
- **Focus:** LinkedIn provider integration, static event display screen, broader usability
- **Provider pattern:** Each provider is a separate project (TagzApp.Providers.{Name}), implements ISocialMediaProvider from TagzApp.Common
- **Provider config:** doc/Provider-Configuration-Pattern.md describes the full pattern
- **Existing providers:** Twitter/X, Mastodon, Bluesky, YouTube, TwitchChat, Blazot, AzureQueue
- **Config admin UI:** TagzApp.Blazor.Client/Components/Admin/{Platform}.Config.Ui.razor
- **Database:** TagzApp.Storage.Postgres (EF Core), TagzApp.Storage.Sqlite.Security
- **Encryption:** AES-256-CBC for sensitive config (doc/Configuration-Encryption.md)
- **Build:** `cd src && dotnet restore && dotnet build TagzApp.Blazor`

## Learnings
- LinkedInConfiguration does NOT extend HttpClientOptions (unlike Mastodon) — LinkedIn needs raw HttpClient with custom Authorization/Version headers per-request, not a pre-configured HttpClient via IServiceCollectionExtensions.AddHttpClient<>
- StartLinkedIn registers HttpClient via IHttpClientFactory default (no HttpClientOptions), so LinkedInProvider calls httpClientFactory.CreateClient(nameof(LinkedInProvider)) and sets headers per-request
- LinkedIn API requires `LinkedIn-Version: 202401` and `X-Restli-Protocol-Version: 2.0.0` headers on every request
- Provider config key pattern: `provider-{id_lowercase}` e.g. `provider-linkedin`
- The `bi-linkedin` Bootstrap Icon auto-resolves via ContentModel.MapProviderToIcon default case — no explicit mapping needed
- IProviderConfiguration.Keys array should NOT include "Enabled" — Enabled is handled implicitly by the config save/load pattern, but GetConfigurationByKey/SetConfigurationByKey must still handle the "Enabled" key
- global.json is pinned to `10.0.100` with `latestPatch` rollForward — the dev environment has 10.0.200-preview installed, so you must temporarily set rollForward to `latestFeature` to build, then revert

## Team Updates
- 📌 **2026-02-18**: LinkedIn provider plan decided by Mercy — architecture approved for implementation. 10 work items across 4 phases (scaffolding, core provider, integration, testing/docs). Estimated 2-3 days for implementation, 1 day for tests/docs.
- 📌 **2026-02-18**: LinkedIn provider implementation completed (Phases 1-3). Configuration uses direct IProviderConfiguration (not HttpClientOptions) for per-request header refresh. Daily API budget tracked in-memory with UTC midnight reset. Wired into Service_Providers.cs and GenericProvider.razor. All 32 unit tests pass. Build: 0 errors. Commit: 2da5f39 — decided by Sombra
