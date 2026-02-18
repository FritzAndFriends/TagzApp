### 2026-02-18: LinkedIn provider open questions resolved
**By:** Jeff Fritz (via Copilot)
**What:**
1. LinkedIn Developer App: ✅ TagzApp already has one — no need to apply for new app
2. Polling frequency: Minimum 5 minutes (app-level tokens). Default to 5 minutes, configurable. 1 minute is not feasible due to 100-500 calls/day rate limits.
3. Icon: Use `bi-linkedin` Bootstrap Icon — confirmed
**Why:** Jeff answered the three open questions from Mercy's LinkedIn provider architecture plan. These decisions unblock Sombra to begin implementation.
