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

## Team Updates
- 📌 **2026-02-18**: LinkedIn provider plan decided by Mercy — architecture approved for implementation. 10 work items across 4 phases (scaffolding, core provider, integration, testing/docs). Estimated 2-3 days for implementation, 1 day for tests/docs.
