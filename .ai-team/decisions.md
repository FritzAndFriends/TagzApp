# Decisions

### 2026-02-18: Team formed — TagzApp advancement
**By:** Squad (Coordinator)
**What:** Hired team (Mercy, Symmetra, Sombra, Ana) to advance TagzApp with LinkedIn integration and static event display.
**Why:** Jeff wants to make TagzApp more accessible and event-friendly. LinkedIn provider adds a major social platform. Static display screen enables hands-free content viewing at conferences and meetups.

### 2026-02-18: LinkedIn Provider Architecture Plan
**By:** Mercy
**What:** Comprehensive architecture plan for adding a LinkedIn social media provider to TagzApp.
**Why:** Jeff wants to expand TagzApp's platform coverage. LinkedIn is a major professional social network, and adding it unlocks hashtag aggregation for tech conferences, .NET community events, and professional meetups where LinkedIn engagement is significant.

---

## 1. Executive Summary

This plan specifies the architecture for `TagzApp.Providers.LinkedIn`, a new social media provider that integrates LinkedIn's content search into TagzApp's hashtag aggregation system. The provider will follow the established TagzApp provider pattern exactly, implementing `ISocialMediaProvider` and `IProviderConfiguration`, registering via `IConfigureProvider`, and adding an admin config UI in the Blazor client.

**Honest assessment:** LinkedIn's API is the most restrictive of any platform TagzApp integrates with. The Community Management API and Marketing API both require LinkedIn partner program approval, and hashtag content search is not available through basic API access. This provider will require a LinkedIn App with elevated permissions, and the approval process can take weeks. The plan accounts for this reality.

---

## 2. LinkedIn API Analysis

### 2.1 Available API Options

**Option A: Marketing API — `GET /rest/posts` (Recommended)**
- Endpoint: `https://api.linkedin.com/rest/posts?q=hashtag&hashtag={encodedHashtag}`
- Returns posts containing a specific hashtag from public LinkedIn content
- Requires: Marketing Developer Platform (MDP) access with `r_organization_social` or `rw_organization_admin` scope
- Returns: Post URN, author info, text content, media attachments, creation timestamp, share statistics
- Rate limit: 100 requests/day per member token (very restrictive), 500/day for app-level
- **Version header required:** `LinkedIn-Version: 202401` (or later)

**Option B: Community Management API — Content search**
- More restrictive; primarily for organization page management
- Not suitable for broad hashtag search across the platform

**Option C: Consumer (Profile) API**
- Only provides authenticated user's own posts and feed
- Cannot search by hashtag; not suitable for TagzApp's use case

### 2.2 Authentication Requirements

- **OAuth 2.0 Authorization Code Flow** (3-legged OAuth)
- App registration at https://www.linkedin.com/developers/
- Required scopes:
  - `r_liteprofile` — Read basic profile info of post authors
  - `r_organization_social` — Read organization posts (for Marketing API hashtag search)
  - `w_member_social` — Not needed, but often bundled
- Access tokens expire in **60 days** (not short-lived like Twitter)
- Refresh tokens available via OAuth 2.0 refresh flow
- **Critical:** The app must be approved for Marketing Developer Platform access to use hashtag search. Basic app registration does NOT include this.

### 2.3 Rate Limits & Constraints

| Endpoint | Limit | Window |
|---|---|---|
| Hashtag post search | 100 calls/day (member), 500/day (app) | 24h rolling |
| Profile lookups | 100 calls/day | 24h rolling |
| General API | 1,000 calls/day (app-level) | 24h rolling |

**Implication for TagzApp:**
- `NewContentRetrievalFrequency` must be very conservative — recommend **every 5 minutes** minimum (288 calls/day at 1-per-5-min)
- With 100 calls/day member limit, we may need to poll every 15+ minutes
- Provider should track daily call budget and degrade gracefully when approaching limit
- Consider caching post IDs to avoid re-processing

### 2.4 Response Data Shape

LinkedIn's Post API returns (relevant fields):
```json
{
  "id": "urn:li:share:1234567890",
  "author": "urn:li:person:ABC123",
  "commentary": "Excited about #dotnetconf! Great talks today.",
  "createdAt": 1700000000000,
  "lifecycleState": "PUBLISHED",
  "visibility": "PUBLIC",
  "content": {
    "media": {
      "id": "urn:li:image:...",
      "altText": "..."
    }
  },
  "distribution": {
    "feedDistribution": "MAIN_FEED"
  }
}
```

Author profile (from `/rest/people/(id:{personId})`):
```json
{
  "id": "ABC123",
  "localizedFirstName": "Jeff",
  "localizedLastName": "Fritz",
  "profilePicture": {
    "displayImage~": {
      "elements": [{ "identifiers": [{ "identifier": "https://..." }] }]
    }
  },
  "vanityName": "jeffreyfritz"
}
```

### 2.5 Known Limitations

1. **No real-time streaming.** LinkedIn has no WebSocket/firehose equivalent. Must poll.
2. **Hashtag search is not guaranteed public.** LinkedIn may filter results to the authenticated user's network, reducing coverage.
3. **Partner approval required.** The Marketing Developer Platform application process can take 2-6 weeks and requires a business justification.
4. **Media access is indirect.** Images/videos are returned as URNs that require separate API calls to resolve to URLs.
5. **Profile picture resolution** requires an additional API call per unique author (cacheable).

---

## 3. Architecture Design

### 3.1 New Project: `TagzApp.Providers.LinkedIn`

**Location:** `src/TagzApp.Providers.LinkedIn/`

**Project file** (`TagzApp.Providers.LinkedIn.csproj`):
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <ItemGroup>
    <ProjectReference Include="..\TagzApp.Common\TagzApp.Common.csproj" />
    <ProjectReference Include="..\TagzApp.Communication\TagzApp.Communication.csproj" />
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" />
    <PackageReference Include="Microsoft.Extensions.Logging.Abstractions" />
    <PackageReference Include="Microsoft.Extensions.Http" />
  </ItemGroup>
  <ItemGroup>
    <Using Include="TagzApp.Common" />
    <Using Include="TagzApp.Common.Models" />
  </ItemGroup>
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
</Project>
```

No third-party LinkedIn SDK needed — the REST API is simple enough to use with `HttpClient` directly, which aligns with how Mastodon does it.

### 3.2 Classes Required

#### `LinkedInConfiguration.cs`
Implements `IProviderConfiguration`. Stores:
- `ClientId` (string) — LinkedIn App Client ID
- `ClientSecret` (string) — LinkedIn App Client Secret (encrypted in DB)
- `AccessToken` (string) — OAuth 2.0 access token (encrypted in DB)
- `RefreshToken` (string) — OAuth 2.0 refresh token (encrypted in DB)
- `TokenExpiresAt` (DateTimeOffset?) — When the current access token expires
- `Enabled` (bool)
- `PollingIntervalMinutes` (int, default 5) — How often to poll for new content
- `DailyCallBudget` (int, default 100) — Max API calls per day

Keys: `["ClientId", "ClientSecret", "AccessToken", "RefreshToken", "TokenExpiresAt", "PollingIntervalMinutes", "DailyCallBudget"]`

Pattern: Follow Mastodon's style — implement `IProviderConfiguration` directly with `GetConfigurationByKey`/`SetConfigurationByKey` switch pattern.

#### `LinkedInProvider.cs`
Implements `ISocialMediaProvider, IDisposable`. Key design:
- `Id` → `"LINKEDIN"`
- `DisplayName` → `"LinkedIn"`
- `Description` → `"LinkedIn professional network hashtag search"`
- `NewContentRetrievalFrequency` → `TimeSpan.FromMinutes(config.PollingIntervalMinutes)` (default 5)
- Uses `HttpClient` to call LinkedIn REST API
- Tracks daily API call count in memory (resets at UTC midnight)
- Caches author profile lookups to minimize API calls (ConcurrentDictionary)
- Maps LinkedIn posts to TagzApp `Content` model
- Handles token refresh automatically when token approaches expiration

**Core polling flow in `GetContentForHashtag`:**
1. Check daily budget — if exhausted, return empty and set health to Degraded
2. URL-encode the hashtag, call `GET /rest/posts?q=hashtag&hashtag={tag}`
3. Filter results by `since` parameter (client-side — LinkedIn doesn't support server-side date filtering well)
4. For each post, resolve author profile (from cache or API)
5. Map to `Content` objects with `Provider = "LINKEDIN"`, `Type = ContentType.Message`
6. `SourceUri` = `https://www.linkedin.com/feed/update/{postUrn}`

#### `LinkedInAuthor.cs` (internal model)
Internal cache model for resolved LinkedIn profiles:
- `PersonUrn` (string)
- `DisplayName` (string)
- `VanityName` (string)
- `ProfileImageUrl` (string)
- `CachedAt` (DateTimeOffset)

#### `StartLinkedIn.cs`
Implements `IConfigureProvider`. Pattern follows `StartBluesky`:
- Loads `LinkedInConfiguration` from `ConfigureTagzAppFactory.Current`
- Registers configuration as singleton
- Registers `LinkedInProvider` as transient `ISocialMediaProvider`
- Registers `HttpClient` for the provider

### 3.3 Config UI Component

**File:** `src/TagzApp.Blazor.Client/Components/Admin/LinkedIn.Config.Ui.razor`

Fields:
- **Client ID** (text input, required)
- **Client Secret** (text input, required — should use password-type display)
- **Access Token** (text input, required — password-type display)
- **Refresh Token** (text input — password-type display)
- **Token Expires At** (read-only display showing when token expires)
- **Polling Interval (minutes)** (numeric input, min 5, default 5)
- **Daily Call Budget** (numeric input, min 10, default 100)
- **Enabled** (checkbox)

Pattern: Follow `Mastodon.Config.Ui.razor` closely — uses `UiProviderConfig` wrapper, `EditForm` with `ViewModel`, saves via `Provider.SaveConfiguration`.

Icon: `bi-linkedin` (Bootstrap Icons includes a LinkedIn icon).

### 3.4 Integration Points (Existing Files to Modify)

These files require surgical additions:

1. **`src/TagzApp.Blazor/Service_Providers.cs`**
   - Add `using TagzApp.Providers.LinkedIn;`
   - Add `new StartLinkedIn()` to the `_Providers` list

2. **`src/TagzApp.Blazor/Components/Admin/Pages/GenericProvider.razor`**
   - Add `@using TagzApp.Providers.LinkedIn`
   - Add case for `nameof(LinkedInProvider)` → render `<LinkedIn_Config_Ui>`

3. **`src/TagzApp.Blazor/TagzApp.Blazor.csproj`**
   - Add `<ProjectReference>` to `TagzApp.Providers.LinkedIn`

4. **`src/TagzApp.ViewModels/Data/ContentModel.cs`** — `MapProviderToIcon`
   - LinkedIn will auto-resolve via the default case: `bi-{provider}` → `bi-linkedin` ✓
   - No change needed unless we want an explicit case for clarity

### 3.5 Database/Storage Implications

- **No schema changes needed.** Provider configuration is stored as JSON in the existing `ConfigureTagzApp` key-value store (same as all other providers). Configuration key: `provider-LINKEDIN`.
- **Sensitive fields** (ClientSecret, AccessToken, RefreshToken) should be encrypted using the existing `EncryptionHelper` in `TagzApp.Common`. The admin UI pattern already handles this — config values pass through the `IConfigureTagzApp` layer which supports encryption.
- **Content** is stored in the existing `Content` table — no new columns needed. LinkedIn posts map cleanly to the existing schema.

### 3.6 Solution File

The new project must be added to `src/TagzApp.sln`:
```
dotnet sln src/TagzApp.sln add src/TagzApp.Providers.LinkedIn/TagzApp.Providers.LinkedIn.csproj
```

---

## 4. Risk Assessment

| Risk | Severity | Mitigation |
|---|---|---|
| LinkedIn partner approval takes weeks | **High** | Begin application immediately. Provider code can be built and tested with mock data while waiting. |
| 100 calls/day rate limit is very low | **Medium** | Conservative polling interval (5-15 min). Daily budget tracking with graceful degradation to `Degraded` health status. |
| Access tokens expire after 60 days | **Medium** | Implement automatic token refresh. Surface token expiry in admin UI and health status. Alert via `Degraded` health when <7 days remain. |
| Hashtag search limited to user's network | **Medium** | Document this limitation clearly. The provider surfaces what LinkedIn gives us — coverage may be narrower than Twitter/Bluesky. |
| LinkedIn API versioning changes | **Low** | Pin `LinkedIn-Version` header. Monitor LinkedIn developer changelog. |
| No real-time streaming | **Low** | Polling is acceptable — YouTube provider works the same way. |

---

## 5. Work Item Breakdown for Sombra (Backend Dev)

All tasks in dependency order. Each builds on the previous.

### Phase 1: Project Scaffolding
**Task 1.1:** Create `src/TagzApp.Providers.LinkedIn/` project
- Create `.csproj` with correct references (TagzApp.Common, TagzApp.Communication)
- Add to solution file
- Add project reference in `TagzApp.Blazor.csproj`

**Task 1.2:** Create `LinkedInConfiguration.cs`
- Implement `IProviderConfiguration`
- All config properties with `GetConfigurationByKey`/`SetConfigurationByKey`
- Configuration key: `provider-LINKEDIN`

### Phase 2: Core Provider
**Task 2.1:** Create `LinkedInProvider.cs`
- Implement `ISocialMediaProvider`
- HttpClient-based API calls with `LinkedIn-Version` header
- `GetContentForHashtag` polling implementation
- Daily API call budget tracking
- Author profile caching (`ConcurrentDictionary`)
- Token expiry monitoring in health checks

**Task 2.2:** Create internal model `LinkedInAuthor.cs`
- Cache model for resolved profile data

**Task 2.3:** Create `StartLinkedIn.cs`
- Implement `IConfigureProvider`
- Register configuration, HttpClient, and provider

### Phase 3: Integration
**Task 3.1:** Wire into `Service_Providers.cs`
- Add using directive and `new StartLinkedIn()` to providers list

**Task 3.2:** Create `LinkedIn.Config.Ui.razor`
- Admin config form in `TagzApp.Blazor.Client/Components/Admin/`
- All config fields with validation
- Follow Mastodon UI pattern

**Task 3.3:** Wire into `GenericProvider.razor`
- Add using directive and switch case for `LinkedInProvider`

### Phase 4: Testing & Polish
**Task 4.1:** Add unit tests in `TagzApp.UnitTest`
- Test configuration serialization/deserialization
- Test content mapping (LinkedIn API response → TagzApp Content)
- Test daily budget tracking logic
- Test token expiry health degradation
- Mock HttpClient for API call tests

**Task 4.2:** Test token refresh flow
- Verify automatic refresh when token approaches expiration
- Verify health degrades gracefully when refresh fails

**Task 4.3:** Documentation
- Add a `ReadMe.md` in the provider project with setup instructions
- Document LinkedIn App creation, MDP application, and required scopes
- Document rate limit implications and polling configuration

---

## 6. Open Questions for Jeff

1. **LinkedIn App access:** Does the TagzApp project already have a LinkedIn Developer App? If not, who will apply for Marketing Developer Platform access? This is a prerequisite for hashtag search.
2. **Polling frequency vs. coverage trade-off:** With 100 calls/day, do you prefer more frequent polling (every 15 min, ~96 calls/day, tighter coverage) or less frequent (every 30 min, ~48 calls/day, more budget headroom)?
3. **Icon:** Bootstrap Icons includes `bi-linkedin`. Is that acceptable, or do you want a custom icon like Bluesky uses (`icon-bluesky`)?

---

## 7. Recommendation

I recommend Sombra starts with Phases 1-3 immediately. The code can be built and integration-tested with mock LinkedIn responses while the LinkedIn Developer App approval is pending. Phase 4 tests should include a comprehensive mock suite so we're not blocked by API access.

The LinkedIn provider is architecturally straightforward — it's a polling HTTP provider like YouTube, not a streaming provider like Bluesky. The main engineering challenges are rate limit management and OAuth token lifecycle, both of which are well-scoped problems.

**Estimated effort:** 2-3 days for a working provider (Phases 1-3), plus 1 day for tests and docs (Phase 4).

---

### 2026-02-18: LinkedIn provider open questions resolved
**By:** Jeff Fritz (via Copilot)
**What:**
1. LinkedIn Developer App: ✅ TagzApp already has one — no need to apply for new app
2. Polling frequency: Minimum 5 minutes (app-level tokens). Default to 5 minutes, configurable. 1 minute is not feasible due to 100-500 calls/day rate limits.
3. Icon: Use `bi-linkedin` Bootstrap Icon — confirmed
**Why:** Jeff answered the three open questions from Mercy's LinkedIn provider architecture plan. These decisions unblock Sombra to begin implementation.

### 2026-02-18: LinkedIn provider does not use HttpClientOptions base class
**By:** Sombra
**What:** LinkedInConfiguration implements IProviderConfiguration directly (not via HttpClientOptions) because LinkedIn's API requires per-request Authorization and versioning headers that change at runtime (token refresh, version bumps). The HttpClient is registered plain via IHttpClientFactory and headers are set on each HttpRequestMessage.
**Why:** The Communication library's AddHttpClient<TClient, TImplementation, TClientOptions> extension requires HttpClientOptions which pre-configures a static BaseAddress, Timeout, and DefaultHeaders. LinkedIn tokens rotate and the Authorization header must be fresh per-request. Using a plain HttpClient avoids coupling to a static config that would go stale on token refresh.

### 2026-02-18: LinkedIn daily API budget tracked in-memory with UTC midnight reset
**By:** Sombra
**What:** Daily API call budget is tracked via an in-memory Interlocked counter that resets when UTC time crosses midnight. No database persistence.
**Why:** Simplicity — the counter resets on app restart anyway, and LinkedIn's own rate limit window is rolling 24h. Persisting to DB would add complexity for minimal value. If the app restarts mid-day, the counter resets to 0, which is conservative (may under-count, never over-count).

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

### 2026-02-18: Waterfall CSS/HTML rendering fixes
**By:** Symmetra
**What:** Fixed 10 rendering issues in the waterfall display: removed `overflow: visible` on hover (content bleed), added word-break for long URLs, constrained card images to `max-width: 100%`, changed modal body from bold to normal weight, fixed footer fade to use CSS variable for theme support, fixed byline overflow on narrow columns, fixed invalid `alt` on `<video>`, fixed stray `}` in overlay alt attribute, fixed double-semicolon in modal display style, added `role="button"` and `tabindex="0"` for keyboard accessibility on waterfall cards.
**Why:** The waterfall is the primary user-facing view at live events. Content bleed on hover, broken word-wrap, unconstrained images, and all-bold modal text all degrade the display quality. Theme-unaware gradient and missing accessibility attributes needed correction for proper dark-mode and keyboard support.

### 2026-02-18: Event Display Page Full-Screen Kiosk Mode
**By:** Symmetra
**What:** Created `/EventDisplay` route with server wrapper (`_EventDisplay.razor`), client component (`EventDisplay.razor`), auto-scroll JavaScript module (`eventDisplay.js`), and event display CSS in `site.css`. Added nav link with `bi-display` icon in Header. Reuses `WaterfallMessage` component for content rendering.
**Why:** Event Display provides a hands-free, full-screen kiosk mode for live events. Auto-scrolling waterfall with branding strip at the bottom eliminates the need for manual control at conferences, meetups, and community events. Follows the established waterfall pattern (SignalR hub connection, content management, InteractiveWebAssembly rendermode) while removing interactive features (no pause button, modal, size controls). Content capped at 50 messages to prevent memory bloat. JavaScript MutationObserver pauses scroll when new content arrives, providing a natural "peek" at fresh posts before resuming the cycle.

### 2025-07-18: LinkedIn OAuth tokens displayed as read-only obfuscated values in admin UI
**By:** Symmetra
**What:** Replaced the editable `<input>` fields for Access Token, Refresh Token, and Token Expires At with read-only obfuscated `<span>` elements in `LinkedIn.Config.Ui.razor`. Removed these three keys from the `SaveConfig()` method so the admin form no longer overwrites provider-managed OAuth tokens. Client ID and Client Secret remain editable — those are user-entered from the LinkedIn Developer Portal.
**Why:** Access Token, Refresh Token, and Token Expires At are managed by the LinkedIn provider's OAuth flow, not entered by the admin user. Allowing form submission to overwrite these values could corrupt a valid OAuth session. Making them display-only prevents accidental data loss while still showing the admin whether tokens are configured. This aligns with Jeff's directive that these are provider-managed, not user-created values.

### 2025-07-18: LinkedIn AntiforgeryToken Fix (Attempt 6)
**By:** Symmetra
**What:** Added `<AntiforgeryToken />` back to LinkedIn.Config.Ui.razor, matching all other working providers. Kept the `_isInteractive` guard on the Save button as a defensive measure.
**Why:** After 5 failed attempts, the root cause was identified: the form needs `<AntiforgeryToken />` for correctness (all other providers have it), AND the Save button must be disabled until the SignalR circuit connects (prevents native HTTP POST during prerender gap). Together these two mechanisms ensure:
1. The antiforgery token is present if a native POST somehow occurs
2. The user cannot submit until the circuit is active, so submissions go through SignalR (not HTTP POST), avoiding antiforgery validation entirely

**Rule for future providers:** Every provider config `<EditForm>` MUST include `<AntiforgeryToken />` immediately after the opening tag. If adding a new provider, copy this pattern from Bluesky/Mastodon.

### 2026-02-19: NEVER modify global.json — SDK version is locked and intentional
**By:** Jeffrey T. Fritz (via Copilot, with clarification session)
**What:** global.json contains Jeff's intentional `rollForward=latestMinor` setting. No agent may modify, revert, or touch global.json under any circumstances. Previous revert (commit e62510f) was a mistake by the coordinator thinking the setting was an unauthorized edit — this broke `blazor.web.js` because the SDK couldn't roll forward to the necessary minor version.
**Why:** User clarified this directive multiple times with increasing urgency. The global.json setting enables SDK minor version flexibility required for TypeScript/Blazor build chain compatibility. Locked configuration prevents runtime failures at live events and conferences where TagzApp is deployed.
**Rule:** NEVER modify global.json. The setting is user-owned infrastructure policy.

### 2026-02-19: LinkedIn OAuth Flow Implementation
**By:** Sombra
**What:** Implemented admin-initiated OAuth 2.0 flow for LinkedIn provider to acquire access tokens and refresh tokens. Created `Service_LinkedInOAuth.cs` with two minimal API endpoints: `/api/linkedin/authorize` (redirects admin to LinkedIn consent) and `/api/linkedin/callback` (receives auth code, exchanges for tokens, persists to LinkedInConfiguration). Updated `LinkedIn.Config.Ui.razor` to add "Authorize with LinkedIn" button and success/error toast notifications. Tokens are stored encrypted via IConfigureTagzApp and expire in 60 days.
**Why:** The LinkedIn provider requires OAuth access tokens to call the LinkedIn Marketing API for hashtag search. Admin users can configure Client ID and Client Secret in the UI, but they cannot manually create access tokens — those must be obtained through OAuth. This implementation follows the 3-legged OAuth 2.0 pattern with state parameter CSRF protection, HTTPS enforcement for callbacks, and proper forwarded headers handling for container/proxy deployments. The flow is admin-specific (not per-user) because the tokens authenticate the TagzApp application itself, not individual users. Without this flow, the LinkedIn provider cannot function even if configured, as it has no way to authenticate API requests.
