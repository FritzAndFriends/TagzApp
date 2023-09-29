# TagzApp
A new website tool that searches social media for hashtags, and tracks chat interaction on several live streaming services

![Sample Screenshot from August 7, 2023](doc/img/Screenshot-2023-09-26.png)

### Overlay display

When some content is selected on the waterfall display, we have an overlay available that can be used with a video capture device

![image](https://github.com/FritzAndFriends/TagzApp/assets/78577/0d7e422a-166a-4d7d-8ea5-ea59f3f4ccbd)


## Current Status

We have completed an initial minimum viable product and stress tested the application by capturing tweets during the NFL kickoff grame on September 7, 2023 between Kansas City and Detroit using the hashtag #DETvsKC

Data is stored in a combination of Sqlite and Postgres databases.  We have configured an extensive provider model so that we can add new social media services in the future.

Live chat integration (TwitchChat, YouTubeChat, etc) captures all messages that are delivered over that service.

We also have a simple moderation capability.

### Currently Supported Services

 - [Blazot](https://www.blazot.com/)
 - Mastodon
 - Twitter / X
 - TwitchChat
 - YouTube (search for videos that have a given hashtag in the description)


