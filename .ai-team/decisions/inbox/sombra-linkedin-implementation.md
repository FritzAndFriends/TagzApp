### 2026-02-18: LinkedIn provider does not use HttpClientOptions base class
**By:** Sombra
**What:** LinkedInConfiguration implements IProviderConfiguration directly (not via HttpClientOptions) because LinkedIn's API requires per-request Authorization and versioning headers that change at runtime (token refresh, version bumps). The HttpClient is registered plain via IHttpClientFactory and headers are set on each HttpRequestMessage.
**Why:** The Communication library's AddHttpClient<TClient, TImplementation, TClientOptions> extension requires HttpClientOptions which pre-configures a static BaseAddress, Timeout, and DefaultHeaders. LinkedIn tokens rotate and the Authorization header must be fresh per-request. Using a plain HttpClient avoids coupling to a static config that would go stale on token refresh.

### 2026-02-18: LinkedIn daily API budget tracked in-memory with UTC midnight reset
**By:** Sombra
**What:** Daily API call budget is tracked via an in-memory Interlocked counter that resets when UTC time crosses midnight. No database persistence.
**Why:** Simplicity — the counter resets on app restart anyway, and LinkedIn's own rate limit window is rolling 24h. Persisting to DB would add complexity for minimal value. If the app restarts mid-day, the counter resets to 0, which is conservative (may under-count, never over-count).
