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
