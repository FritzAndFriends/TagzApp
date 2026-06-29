# <img alt="TagzApp logo" src="doc/img/Tagzap_level_transparent-700h.webp" height="200" />

A website tool that searches social media for hashtags, and tracks chat interaction on several live streaming services

<!-- ALL-CONTRIBUTORS-BADGE:START - Do not remove or modify this section -->
[![All Contributors](https://img.shields.io/badge/all_contributors-21-orange.svg?style=flat-square)](#contributors-)
<!-- ALL-CONTRIBUTORS-BADGE:END -->

![Screenshot from .NET Conf 2024](doc/img/dotnetconf2024-inuse.png)

### Overlay display

When some content is selected on the waterfall display, we have an overlay available that can be used with a video capture device

![image](https://github.com/FritzAndFriends/TagzApp/assets/78577/0d7e422a-166a-4d7d-8ea5-ea59f3f4ccbd)


## Current Features

TagzApp has been successfully deployed and proven in production for major .NET community events, having powered the social media aggregation for both **.NET Conf 2023** and **.NET Conf 2024**. The application has demonstrated its reliability and scalability in handling high-volume, real-time social media content during these large-scale developer conferences.

### Production-Proven Features

#### Social Media Provider Support
TagzApp currently supports **seven different social media providers**, offering comprehensive coverage across major platforms:
- **[Blazot](https://www.blazot.com/)** - Developer-focused social platform
- **Bluesky** - Decentralized social networking
- **Mastodon** - Federated social network  
- **Twitter/X** - Major social media platform
- **TwitchChat** - Live streaming chat integration
- **YouTube** - Video search by hashtag in descriptions
- **YouTube Live Chat** - Real-time chat during live streams
- **Azure Queue Integration** - Custom message delivery for website integration

#### Advanced Moderation Capabilities
The application features a comprehensive moderation system designed for high-traffic events:

- **Multi-layered content review** with human moderator oversight
- **Keyboard navigation support** for efficient moderation workflows
- **Real-time approval/rejection** with immediate visual feedback
- **Bulk moderation actions** for managing high-volume content streams
- **User blocking capabilities** with granular control options

#### Azure Content Safety Integration
TagzApp incorporates **automated AI-powered moderation** through Azure Content Safety:

- **Automated initial screening** of all incoming content across four critical categories:
  - Sexual content detection
  - Hate speech identification  
  - Self-harm content flagging
  - Violence-related content filtering
- **Severity-based automatic rejection** with configurable thresholds
- **Human moderator override capabilities** for nuanced decision-making
- **Detailed AI reasoning** provided to moderators for informed review
- **24/7 automated protection** ensuring continuous content monitoring

#### Professional Overlay System
Multiple overlay formats are available for live streaming and broadcast integration:

- **Landscape Overlay** (`/overlay`) - Optimized for widescreen displays and OBS integration
- **Portrait Overlay** (`/PortraitOverlay`) - Designed for mobile-friendly and vertical layouts  
- **Real-time content selection** - Content selected on the waterfall automatically appears on overlays
- **Video capture device compatibility** - Seamless integration with streaming software
- **Customizable styling** with CSS configuration options
- **Preview capabilities** for testing before going live

#### Technical Architecture
Built on modern .NET technologies for scalability and performance:

- **.NET 9** with Blazor Server and WebAssembly hybrid architecture
- **SignalR** for real-time communication and live updates
- **PostgreSQL** for robust data storage and management
- **.NET Aspire** for cloud-native application orchestration
- **Azure integration** for cloud deployment and scaling
- **Docker support** for containerized deployments

#### Features Ready for Your Next Event
- **Role-based access control** with admin and moderator permissions
- **Single-user mode** for smaller events or private instances  
- **Extensive configuration management** with database-backed settings
- **Telemetry and monitoring** integration for production observability
- **Responsive design** optimized for desktop and mobile moderation workflows

## Contributing or running in development mode

If you want to run your own version of TagzApp locally, check out the [CONTRIBUTING.md](CONTRIBUTING.md) document

## Contributors ✨

<!-- ALL-CONTRIBUTORS-LIST:START - Do not remove or modify this section -->
<!-- prettier-ignore-start -->
<!-- markdownlint-disable -->
<table>
  <tbody>
    <tr>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/csharpfritz"><img src="https://github.com/csharpfritz.png?size=100" width="100px;" alt="Jeffrey T. Fritz"/><br /><sub><b>Jeffrey T. Fritz</b></sub></a><br /><a href="https://github.com/FritzAndFriends/TagzApp/commits?author=csharpfritz" title="Code">💻</a></td>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/degenone"><img src="https://github.com/degenone.png?size=100" width="100px;" alt="Tero Kilpelainen"/><br /><sub><b>Tero Kilpelainen</b></sub></a><br /><a href="https://github.com/FritzAndFriends/TagzApp/commits?author=degenone" title="Code">💻</a></td>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/Stelzi79"><img src="https://github.com/Stelzi79.png?size=100" width="100px;" alt="Stelzi79"/><br /><sub><b>Stelzi79</b></sub></a><br /><a href="https://github.com/FritzAndFriends/TagzApp/commits?author=Stelzi79" title="Code">💻</a></td>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/NapalmCodes"><img src="https://github.com/NapalmCodes.png?size=100" width="100px;" alt="Shawn Vause"/><br /><sub><b>Shawn Vause</b></sub></a><br /><a href="https://github.com/FritzAndFriends/TagzApp/commits?author=NapalmCodes" title="Code">💻</a></td>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/CZEMacLeod"><img src="https://github.com/CZEMacLeod.png?size=100" width="100px;" alt="Cynthia MacLeod"/><br /><sub><b>Cynthia MacLeod</b></sub></a><br /><a href="https://github.com/FritzAndFriends/TagzApp/commits?author=CZEMacLeod" title="Code">💻</a></td>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/ElliottBrand"><img src="https://github.com/ElliottBrand.png?size=100" width="100px;" alt="Steve Elliott"/><br /><sub><b>Steve Elliott</b></sub></a><br /><a href="https://github.com/FritzAndFriends/TagzApp/commits?author=ElliottBrand" title="Code">💻</a></td>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/JBraunsmaJr"><img src="https://github.com/JBraunsmaJr.png?size=100" width="100px;" alt="Badger"/><br /><sub><b>Badger</b></sub></a><br /><a href="https://github.com/FritzAndFriends/TagzApp/commits?author=JBraunsmaJr" title="Code">💻</a></td>
    </tr>
    <tr>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/eric-vanartsdalen"><img src="https://github.com/eric-vanartsdalen.png?size=100" width="100px;" alt="Eric VanArtsdalen"/><br /><sub><b>Eric VanArtsdalen</b></sub></a><br /><a href="https://github.com/FritzAndFriends/TagzApp/commits?author=eric-vanartsdalen" title="Code">💻</a></td>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/paddybhoy66"><img src="https://github.com/paddybhoy66.png?size=100" width="100px;" alt="Pat Woods"/><br /><sub><b>Pat Woods</b></sub></a><br /><a href="https://github.com/FritzAndFriends/TagzApp/commits?author=paddybhoy66" title="Code">💻</a></td>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/SQL-MisterMagoo"><img src="https://github.com/SQL-MisterMagoo.png?size=100" width="100px;" alt="SQL-MisterMagoo"/><br /><sub><b>SQL-MisterMagoo</b></sub></a><br /><a href="https://github.com/FritzAndFriends/TagzApp/commits?author=SQL-MisterMagoo" title="Code">💻</a></td>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/mpaulosky"><img src="https://github.com/mpaulosky.png?size=100" width="100px;" alt="mpaulosky"/><br /><sub><b>mpaulosky</b></sub></a><br /><a href="https://github.com/FritzAndFriends/TagzApp/commits?author=mpaulosky" title="Code">💻</a></td>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/mcNets"><img src="https://github.com/mcNets.png?size=100" width="100px;" alt="Joan Magnet"/><br /><sub><b>Joan Magnet</b></sub></a><br /><a href="https://github.com/FritzAndFriends/TagzApp/commits?author=mcNets" title="Code">💻</a></td>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/johanbenschop"><img src="https://github.com/johanbenschop.png?size=100" width="100px;" alt="Johan Benschop"/><br /><sub><b>Johan Benschop</b></sub></a><br /><a href="https://github.com/FritzAndFriends/TagzApp/commits?author=johanbenschop" title="Code">💻</a></td>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/jedelfraisse"><img src="https://github.com/jedelfraisse.png?size=100" width="100px;" alt="Jonathan Delfraisse"/><br /><sub><b>Jonathan Delfraisse</b></sub></a><br /><a href="https://github.com/FritzAndFriends/TagzApp/commits?author=jedelfraisse" title="Code">💻</a></td>
    </tr>
    <tr>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/justinhhorner"><img src="https://github.com/justinhhorner.png?size=100" width="100px;" alt="Justin Horner"/><br /><sub><b>Justin Horner</b></sub></a><br /><a href="https://github.com/FritzAndFriends/TagzApp/commits?author=justinhhorner" title="Code">💻</a></td>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/NickSpaghetti"><img src="https://github.com/NickSpaghetti.png?size=100" width="100px;" alt="Nick Spaghetti"/><br /><sub><b>Nick Spaghetti</b></sub></a><br /><a href="https://github.com/FritzAndFriends/TagzApp/commits?author=NickSpaghetti" title="Code">💻</a></td>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/mogas"><img src="https://github.com/mogas.png?size=100" width="100px;" alt="Nuno Silva"/><br /><sub><b>Nuno Silva</b></sub></a><br /><a href="https://github.com/FritzAndFriends/TagzApp/commits?author=mogas" title="Code">💻</a></td>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/szalapski"><img src="https://github.com/szalapski.png?size=100" width="100px;" alt="Patrick Szalapski"/><br /><sub><b>Patrick Szalapski</b></sub></a><br /><a href="https://github.com/FritzAndFriends/TagzApp/commits?author=szalapski" title="Code">💻</a></td>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/acidrod"><img src="https://github.com/acidrod.png?size=100" width="100px;" alt="Rodrigo Alarcon"/><br /><sub><b>Rodrigo Alarcon</b></sub></a><br /><a href="https://github.com/FritzAndFriends/TagzApp/commits?author=acidrod" title="Code">💻</a></td>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/ryanshaut"><img src="https://github.com/ryanshaut.png?size=100" width="100px;" alt="Ryan Shaut"/><br /><sub><b>Ryan Shaut</b></sub></a><br /><a href="https://github.com/FritzAndFriends/TagzApp/commits?author=ryanshaut" title="Code">💻</a></td>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/ThindalTV"><img src="https://github.com/ThindalTV.png?size=100" width="100px;" alt="Thindal"/><br /><sub><b>Thindal</b></sub></a><br /><a href="https://github.com/FritzAndFriends/TagzApp/commits?author=ThindalTV" title="Code">💻</a></td>
    </tr>
  </tbody>
</table>

<!-- markdownlint-restore -->
<!-- prettier-ignore-end -->

<!-- ALL-CONTRIBUTORS-LIST:END -->
