# Routing Rules

| Domain | Agent | Examples |
|--------|-------|----------|
| Architecture, scope, code review | Mercy 🏗️ | Design decisions, PR reviews, trade-offs |
| Blazor UI, components, display views, CSS, layout | Symmetra ⚛️ | Waterfall display, static screen, overlay, Razor components |
| APIs, providers, data, LinkedIn, social media integration | Sombra 🔧 | LinkedIn provider, ISocialMediaProvider, API clients, EF migrations |
| Tests, quality, edge cases, validation | Ana 🧪 | Unit tests, integration tests, Playwright tests, test coverage |
| Session logs, decisions, memory | Scribe 📋 | Automatic — after every batch |
| Work queue, backlog, keep-alive | Ralph 🔄 | Activated on request |

## Escalation

- Ambiguous → Mercy triages
- Multi-domain → Fan out to relevant agents in parallel
- Provider + UI together → Sombra (provider) + Symmetra (UI) in parallel
