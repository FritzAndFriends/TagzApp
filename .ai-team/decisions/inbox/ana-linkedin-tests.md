### 2026-02-18: LinkedIn Provider Unit Tests Written (Phase 4.1)
**By:** Ana
**What:** Created `src/TagzApp.UnitTest/LinkedIn/LinkedInProviderTests.cs` with 20 test cases covering configuration, content mapping, daily budget tracking, token expiry health, and provider metadata. Added project reference to `TagzApp.Providers.LinkedIn` in `TagzApp.UnitTest.csproj`.
**Why:** Tests are written in parallel with Sombra's implementation (Phases 1-3) per the architecture plan. Tests will compile once the LinkedIn provider project exists.

**Test assumptions (Sombra must match):**
- `LinkedInProvider` constructor: `(IHttpClientFactory, LinkedInConfiguration)`
- `LinkedInConfiguration` keys: `ClientId`, `ClientSecret`, `AccessToken`, `RefreshToken`, `TokenExpiresAt`, `PollingIntervalMinutes`, `DailyCallBudget`
- `PollingIntervalMinutes` default: 5; `DailyCallBudget` default: 100
- `Id` = `"LINKEDIN"`, `DisplayName` = `"LinkedIn"`
- Health logic: token >7 days → Healthy, <7 days → Degraded, expired → Unhealthy, budget exhausted → Degraded
- `GetContentForHashtag` returns empty when budget exhausted
- Content mapping: `SourceUri` = `https://www.linkedin.com/feed/update/{postUrn}`, `Provider` = `"LINKEDIN"`, text from `commentary` field

**Status:** Tests written, not yet compilable (awaiting Sombra's provider project).
