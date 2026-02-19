### LinkedIn AntiforgeryToken Fix (Attempt 6)
**By:** Symmetra
**Date:** 2025-07-18
**What:** Added `<AntiforgeryToken />` back to LinkedIn.Config.Ui.razor, matching all other working providers. Kept the `_isInteractive` guard on the Save button as a defensive measure.
**Why:** After 5 failed attempts, the root cause was identified: the form needs `<AntiforgeryToken />` for correctness (all other providers have it), AND the Save button must be disabled until the SignalR circuit connects (prevents native HTTP POST during prerender gap). Together these two mechanisms ensure:
1. The antiforgery token is present if a native POST somehow occurs
2. The user cannot submit until the circuit is active, so submissions go through SignalR (not HTTP POST), avoiding antiforgery validation entirely
**Rule for future providers:** Every provider config `<EditForm>` MUST include `<AntiforgeryToken />` immediately after the opening tag. If adding a new provider, copy this pattern from Bluesky/Mastodon.
