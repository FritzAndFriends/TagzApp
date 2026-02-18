# Symmetra — History

## Project Context
- **Project:** TagzApp — .NET 9.0 Blazor Server app for social media hashtag aggregation
- **Stack:** C#, .NET 9, Blazor Server/WASM, SignalR, Entity Framework, PostgreSQL/SQLite, .NET Aspire
- **User:** Jeffrey T. Fritz (Jeff)
- **Focus:** LinkedIn provider integration, static event display screen, broader usability
- **UI framework:** Bootstrap 5 with Bootstrap Icons (MIT). No other icon/CSS libraries allowed.
- **Main UI project:** TagzApp.Blazor (server-side Blazor), TagzApp.Blazor.Client (WebAssembly components)
- **Key UI paths:** src/TagzApp.Blazor/Components/ for server components, src/TagzApp.Blazor.Client/Components/ for client components
- **SignalR hubs:** src/TagzApp.Blazor/Hubs/ for real-time content updates
- **Routing:** src/TagzApp.Blazor/Components/App.razor

## Learnings
- Waterfall uses a CSS Grid masonry layout (`#taggedContent`) with `grid-auto-rows: 10px` and JS-based row-span resizing (Masonry.resizeGridItem). Changing overflow or layout properties on articles can break the grid.
- The main waterfall CSS lives in `src/TagzApp.Blazor/wwwroot/css/site.css` — it covers #taggedContent, article cards, overlay display, portrait overlay, modal display, and moderation UI.
- Waterfall JS interop: `wwwroot/js/waterfall.js` (WaterfallUi) and `wwwroot/js/waterfallSizeControl.js` (WaterfallSizeControl).
- Key waterfall components: `_Waterfall.razor` (server page), `Waterfall.razor` (client WASM), `WaterfallMessage.razor` (card), `WaterfallModal.razor` (detail modal), `WaterfallSizeControl.razor` (tile/modal sizing).
- `WaterfallLayout.razor` sets `body { overflow-y: hidden; }` to prevent body scrolling — the scroll container is `#taggedContent` itself.
- The modal flip animation uses `scale: 1.4` on `.modal-front/.modal-back` — this is intentional for the card flip reveal effect.
- `#footerFade` uses absolute positioning inside a 100vh wrapper to create a gradient fade at the bottom of the waterfall scroll area.
- Overlay display (`Overlay.razor`) is a separate page with green-screen background for OBS capture at events.
- Event Display page pattern: server page (`_EventDisplay.razor`) wraps client component (`EventDisplay.razor`) with InteractiveWebAssembly rendermode. SignalR connection via `/messages?t={tag}` hub, listen for `NewWaterfallMessage` and `RemoveMessage` events.
- Event Display reuses `WaterfallMessage` component without click handlers (no `OnContentSelected` callback) for hands-free operation. Content capped at 50 messages with oldest removed when limit exceeded.
- Auto-scroll JavaScript pattern: namespace-based module (`window.EventDisplay`), MutationObserver for detecting new content, pause/resume logic with configurable intervals. Calls `window.Masonry.setupPage` if available to maintain grid layout.
- Bootstrap Icons: `bi-display` used for Event Display nav link. Never introduce other icon libraries — Bootstrap Icons only.

## Team Updates
- 📌 **2026-02-18**: Waterfall CSS/HTML rendering fixes completed. Fixed 10 rendering issues: overflow bleed on hover, word-break for long URLs, constrained card images, modal text weight, footer fade CSS variable, byline overflow, video alt attribute, overlay alt stray character, modal display semicolon, card keyboard accessibility (role="button" + tabindex). All fixes improve display quality at live events and support dark-mode/keyboard navigation. Commit: 2da5f39 — decided by Symmetra
- 📌 **2026-02-18**: Event Display page full-screen kiosk mode created. Added `/EventDisplay` route with server wrapper, client WASM component, auto-scroll JS module with MutationObserver pause/resume, and dedicated CSS. Reuses WaterfallMessage component for hands-free operation (no click handlers). Content capped at 50 messages. Includes nav link with `bi-display` icon. Enables hands-free event viewing at conferences and meetups. Commit: bae3984 — decided by Symmetra
